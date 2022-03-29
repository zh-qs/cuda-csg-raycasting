
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include <cmath>

#include <thrust/sort.h>


#define OP_UNION 1
#define OP_INTERSECTION 2
#define OP_DIFFERENCE 0
#define OP_SPHERE 3

#define GET_OP_ID(flag) (flag & 0b11)
#define SET_OP_ID(flag, op) (flag = (flag & 0xfffffff8) | op)

#define IS_NEW_SEGMENT(i) (scene.tree.flags[i] & 0b100)

#define GET_INTERSECTION_ID_FROM_FLAG(flag) ((flag >> 16) | ((threadIdx.x / scene.tree.n) << 16))
#define GET_INTERSECTION_ID(i) GET_INTERSECTION_ID_FROM_FLAG(scene.tree.flags[i])
#define GET_INTERSECTION_COUNT(i) ((scene.tree.flags[i] & 0xffff) >> 3) 

#define MAKE_FLAG(intId, intCount, newSeg, op) ((intId << 16) | (intCount << 3) | (newSeg << 2) | op)

//#define FRAGMENT_GREATER(val1, i1, val2, i2) (GET_INTERSECTION_ID(i1) > GET_INTERSECTION_ID(i2)) || (GET_INTERSECTION_ID(i1) == GET_INTERSECTION_ID(i2) && val1 > val2)
#define FRAGMENT_GREATER(val1, i1, val2, i2) (i1 > i2) || (i1 == i2 && val1 > val2)

struct normalisedCsgTree
{
    int n;
    int* flags;
    int* color;
    float* x, * y, * z;
    float* radius;
};

#define TREE_SIZE(t) (2 * sizeof(int) + 4 * sizeof(float)) * t.n
#define TREE_INTS(t) 2 * t.n
#define TREE_FLOATS(t) 4 * t.n

struct light
{
    float ka, kd, ks, m;
};

struct sceneParameters
{
    normalisedCsgTree tree;
    float3 observer;
    float3 direction;
    float3 planeHorizontal;
    float3 unitVectorToLight;
    int width;
    int height;
    float planeHeight;
    float planeDistance;
    light light;
};


__constant__ struct sceneParameters scene {};

//const __device__ float planeHeight = 1, planeDistance = 0.5; // planeDistance - odległość od obserwatora do rzutni

__device__ float3 normalised(float3 &vec)
{
    float d = sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
    return make_float3(vec.x / d, vec.y / d, vec.z / d);
}

__device__ float3 minusNormalised(float3 &vec)
{
    float d = -sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
    return make_float3(vec.x / d, vec.y / d, vec.z / d);
}

__device__ float length(float3 &vec)
{
    return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
}

__device__ float scalarProduct(float3 &v1, float3 &v2)
{
    return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
}

__device__ void calculateIntersection(int i, float *frontFace, float *backFace, float3 &vecTopointOnScene)
{
    float x = scene.tree.x[i], y = scene.tree.y[i], z = scene.tree.z[i], radius = scene.tree.radius[i];
    //http://paulbourke.net/geometry/circlesphere/index.html#linesphere
    float a = vecTopointOnScene.x * vecTopointOnScene.x + vecTopointOnScene.y * vecTopointOnScene.y + vecTopointOnScene.z * vecTopointOnScene.z,
        b = 2 * (vecTopointOnScene.x * (scene.observer.x - x) + vecTopointOnScene.y * (scene.observer.y - y) + vecTopointOnScene.z * (scene.observer.z - z)),
        c = x * x + y * y + z * z + scene.observer.x * scene.observer.x + scene.observer.y * scene.observer.y + scene.observer.z * scene.observer.z - 2 * (x * scene.observer.x + y * scene.observer.y + z * scene.observer.z) - radius * radius;

    float delta = b * b - 4 * a * c;
    if (delta >= 0)
    {
        *frontFace = (-b - sqrt(delta)) / 2 / a;
        *backFace = (-b + sqrt(delta)) / 2 / a;
    }
    else
    {
        *frontFace = INFINITY;
        *backFace = -INFINITY;
    }
}


template <class T>
__device__ void swap(T &x, T &y)
{
    T p = x;
    x = y;
    y = p;
}

// fragment-sorts shm_faces of one block size
__device__ void fragmentSortInBlock(volatile float* shm_faces, volatile int* shm_indexes, volatile int* shm_intersectionIds, volatile int* shm_a_to_scan)
{
    const unsigned int tid = threadIdx.x;
    __syncthreads();
    for (unsigned int k = 2; k <= blockDim.x; k <<= 1)
        for (unsigned int j = k >> 1; j > 0; j >>= 1)
        {
            unsigned int ixj = tid ^ j;
            if (ixj > tid) {
                if ((tid & k) == 0) {
                    //if (FRAGMENT_GREATER(shm_faces[shm_perm[tid]], shm_intersectionIds[shm_perm[tid]], shm_faces[shm_perm[ixj]], shm_intersectionIds[shm_perm[ixj]]))
                    if (FRAGMENT_GREATER(shm_faces[tid], shm_intersectionIds[tid], shm_faces[ixj], shm_intersectionIds[ixj]))
                    {
                        //swap(shm_perm[tid], shm_perm[ixj]);
                        swap(shm_faces[tid], shm_faces[ixj]);
                        swap(shm_indexes[tid], shm_indexes[ixj]);
                        swap(shm_intersectionIds[tid], shm_intersectionIds[ixj]);
                        swap(shm_a_to_scan[tid], shm_a_to_scan[ixj]);
                    }
                }
                else {
                    //if (FRAGMENT_GREATER(shm_faces[shm_perm[ixj]], shm_intersectionIds[shm_perm[ixj]], shm_faces[shm_perm[tid]], shm_intersectionIds[shm_perm[tid]]))
                    if (FRAGMENT_GREATER(shm_faces[ixj], shm_intersectionIds[ixj], shm_faces[tid], shm_intersectionIds[tid]))
                    {
                        //swap(shm_perm[tid], shm_perm[ixj]);
                        swap(shm_faces[tid], shm_faces[ixj]);
                        swap(shm_indexes[tid], shm_indexes[ixj]);
                        swap(shm_intersectionIds[tid], shm_intersectionIds[ixj]); 
                        swap(shm_a_to_scan[tid], shm_a_to_scan[ixj]);
                    }
                }
            }
            __syncthreads();
        }
    __syncthreads();
}
//__device__ void fragmentSortInBlockC(volatile float* shm_faces, volatile int* shm_indexes, volatile int* shm_intersectionIds, volatile int* shm_a_to_scan)
//{
//    const unsigned int tid = threadIdx.x;
//    __syncthreads();
//    for (unsigned int k = 2; k <= blockDim.x; k <<= 1)
//        for (unsigned int j = k >> 1; j > 0; j >>= 1)
//        {
//            unsigned int ixj = tid ^ j;
//            printf("%d %d %d %d\n", shm_indexes[tid], shm_indexes[ixj], tid, ixj);
//            if (ixj > tid) {
//                if ((tid & k) == 0) {
//                    if (FRAGMENT_GREATER(shm_faces[tid], shm_intersectionIds[tid], shm_faces[ixj], shm_intersectionIds[ixj]))
//                    {
//                        swap(shm_faces[tid], shm_faces[ixj]);
//                        swap(shm_indexes[tid], shm_indexes[ixj]);
//                        swap(shm_intersectionIds[tid], shm_intersectionIds[ixj]);
//                        swap(shm_a_to_scan[tid], shm_a_to_scan[ixj]);
//                    }
//                }
//                else {
//                    if (FRAGMENT_GREATER(shm_faces[ixj], shm_intersectionIds[ixj], shm_faces[tid], shm_intersectionIds[tid]))
//                    {
//                        swap(shm_faces[tid], shm_faces[ixj]);
//                        swap(shm_indexes[tid], shm_indexes[ixj]);
//                        swap(shm_intersectionIds[tid], shm_intersectionIds[ixj]);
//                        swap(shm_a_to_scan[tid], shm_a_to_scan[ixj]);
//                    }
//                }
//            }
//            __syncthreads();
//        }
//    __syncthreads();
//}

// merges shm_faces of 2*blockSize, when shm_faces[0...blockSize-1] and shm_faces[blockSize,2*blockSize-1] are fragment-sorted
__device__ void mergeTwoBlockSizeArrays(volatile float* shm_faces, volatile int* shm_indexes, volatile int* shm_intersectionIds, volatile int* shm_a_to_scan)
{
    const unsigned int tid = threadIdx.x;
    __syncthreads();
    //if (FRAGMENT_GREATER(shm_faces[shm_perm[tid]], shm_intersectionIds[shm_perm[tid]], shm_faces[shm_perm[2 * blockDim.x - tid - 1]], shm_intersectionIds[shm_perm[2 * blockDim.x - tid - 1]]))
    if (FRAGMENT_GREATER(shm_faces[tid], shm_intersectionIds[tid], shm_faces[2 * blockDim.x - tid - 1], shm_intersectionIds[2 * blockDim.x - tid - 1]))
    {
        
        //swap(shm_perm[tid], shm_perm[2 * blockDim.x - tid - 1]);
        swap(shm_faces[tid], shm_faces[2 * blockDim.x - tid - 1]);
        swap(shm_indexes[tid], shm_indexes[2 * blockDim.x - tid - 1]);
        swap(shm_intersectionIds[tid], shm_intersectionIds[2 * blockDim.x - tid - 1]);
        swap(shm_a_to_scan[tid], shm_a_to_scan[2 * blockDim.x - tid - 1]);
    }
    __syncthreads();
    for (unsigned int j = blockDim.x >> 1; j > 0; j >>= 1)
    {
        unsigned int ixj = tid ^ j;
        if (ixj > tid) {
            if (FRAGMENT_GREATER(shm_faces[tid], shm_intersectionIds[tid], shm_faces[ixj], shm_intersectionIds[ixj]))
            //if (FRAGMENT_GREATER(shm_faces[shm_perm[tid]], shm_intersectionIds[shm_perm[tid]], shm_faces[shm_perm[ixj]], shm_intersectionIds[shm_perm[ixj]]))
            {
                //swap(shm_perm[tid], shm_perm[ixj]);
                swap(shm_faces[tid], shm_faces[ixj]);
                swap(shm_indexes[tid], shm_indexes[ixj]);
                swap(shm_intersectionIds[tid], shm_intersectionIds[ixj]);
                swap(shm_a_to_scan[tid], shm_a_to_scan[ixj]);
            }
        }
        else {
            ixj += blockDim.x;
            //if (FRAGMENT_GREATER(shm_faces[shm_perm[ixj]], shm_intersectionIds[shm_perm[ixj]], shm_faces[shm_perm[tid + blockDim.x]], shm_intersectionIds[shm_perm[tid + blockDim.x]]))
            if (FRAGMENT_GREATER(shm_faces[ixj], shm_intersectionIds[ixj], shm_faces[tid + blockDim.x], shm_intersectionIds[tid + blockDim.x]))
            {
                //swap(shm_perm[ixj], shm_perm[tid + blockDim.x]);
                swap(shm_faces[tid + blockDim.x], shm_faces[ixj]);
                swap(shm_indexes[tid + blockDim.x], shm_indexes[ixj]);
                swap(shm_intersectionIds[tid + blockDim.x], shm_intersectionIds[ixj]);
                swap(shm_a_to_scan[tid + blockDim.x], shm_a_to_scan[ixj]);
            }
        }

        __syncthreads();
    }
}

// z pracy Efficient Parallel Sacn Algorithms for GPUs, S. Sengupta, M. Harris, M. Garland
__device__ int segscan_warp(volatile int* ptr, volatile int* hd)
{
    const unsigned int idx = threadIdx.x, lane = idx & 31;
    if (lane >= 1) {
        ptr[idx] = hd[idx] ? ptr[idx] : (ptr[idx - 1] + ptr[idx]);
        hd[idx] = hd[idx - 1] | hd[idx];
    }
    if (lane >= 2) {
        ptr[idx] = hd[idx] ? ptr[idx] : (ptr[idx - 2] + ptr[idx]);
        hd[idx] = hd[idx - 2] | hd[idx];
    }
    if (lane >= 4) {
        ptr[idx] = hd[idx] ? ptr[idx] : (ptr[idx - 4] + ptr[idx]);
        hd[idx] = hd[idx - 4] | hd[idx];
    }
    if (lane >= 8) {
        ptr[idx] = hd[idx] ? ptr[idx] : (ptr[idx - 8] + ptr[idx]);
        hd[idx] = hd[idx - 8] | hd[idx];
    }
    if (lane >= 16) {
        ptr[idx] = hd[idx] ? ptr[idx] : (ptr[idx - 16] + ptr[idx]);
        hd[idx] = hd[idx - 16] | hd[idx];
    }
    return ptr[idx];
}

__device__ int segscan_block(volatile int* ptr, volatile int* hd)
{
    const unsigned int idx = threadIdx.x;
    unsigned int warpid = idx >> 5;
    unsigned int warp_first = warpid << 5;
    unsigned int warp_last = warp_first + 31;
    // Step 1a:
    // Before overwriting the input head flags , record whether
    // this warp begins with an "open" segment.
    bool warp_is_open = (hd[warp_first] == 0);
    __syncthreads();
    // Step 1b:
    // Intra -warp segmented scan in each warp.
    int val = segscan_warp(ptr, hd);
    // Step 2a:
    // Since ptr [] contains *inclusive * results , irrespective of Kind ,
    // the last value is the correct partial result.
    int warp_total = ptr[warp_last];
    // Step 2b:
    // warp_flag is the OR -reduction of the flags in a warp and is
    // computed indirectly from the mindex values in hd [].
    // will_accumulate indicates that a thread will only accumulate a
    // partial result in Step 4 if there is no segment boundary to its left.
    bool warp_flag = hd[warp_last] != 0 || !warp_is_open;
    bool will_accumulate = warp_is_open && hd[idx] == 0;
    __syncthreads();
    // Step 2c: The last thread in each warp writes partial results
    if (idx == warp_last)
    {
        ptr[warpid] = warp_total;
        hd[warpid] = warp_flag;
    }
    __syncthreads();
    // Step 3: One warp scans the per -warp results
    if (warpid == 0)
        segscan_warp(ptr, hd);
    __syncthreads();
    // Step 4: Accumulate results from Step 3, as appropriate .
    if (warpid != 0 && will_accumulate)
        val = ptr[warpid - 1] + val;
    __syncthreads();
    ptr[idx] = val;
    __syncthreads();
    return val;
}

__device__ float segscan_min_warp(volatile float* ptr, volatile int* hd, volatile int* indexes)
{
    const unsigned int idx = threadIdx.x, lane = idx & 31;
    /*if (lane >= 1) {
        ptr[idx] = hd[idx] ? ptr[idx] : fminf(ptr[idx - 1], ptr[idx]);
        hd[idx] = hd[idx - 1] | hd[idx];
    }
    if (lane >= 2) {
        ptr[idx] = hd[idx] ? ptr[idx] : fminf(ptr[idx - 2], ptr[idx]);
        hd[idx] = hd[idx - 2] | hd[idx];
    }
    if (lane >= 4) {
        ptr[idx] = hd[idx] ? ptr[idx] : fminf(ptr[idx - 4], ptr[idx]);
        hd[idx] = hd[idx - 4] | hd[idx];
    }
    if (lane >= 8) {
        ptr[idx] = hd[idx] ? ptr[idx] : fminf(ptr[idx - 8], ptr[idx]);
        hd[idx] = hd[idx - 8] | hd[idx];
    }
    if (lane >= 16) {
        ptr[idx] = hd[idx] ? ptr[idx] : fminf(ptr[idx - 16], ptr[idx]);
        hd[idx] = hd[idx - 16] | hd[idx];
    }*/
    if (lane >= 1)
    {
        if (!hd[idx] && ptr[idx - 1] < ptr[idx])
        {
            ptr[idx] = ptr[idx - 1];
            indexes[idx] = indexes[idx - 1];
        }
        hd[idx] = hd[idx - 1] | hd[idx];
    }
    if (lane >= 2)
    {
        if (!hd[idx] && ptr[idx - 2] < ptr[idx])
        {
            ptr[idx] = ptr[idx - 2];
            indexes[idx] = indexes[idx - 2];
        }
        hd[idx] = hd[idx - 2] | hd[idx];
    }
    if (lane >= 4)
    {
        if (!hd[idx] && ptr[idx - 4] < ptr[idx]) 
        {
            ptr[idx] = ptr[idx - 4];
            indexes[idx] = indexes[idx - 4];
        }
        hd[idx] = hd[idx - 4] | hd[idx];
    }
    if (lane >= 8)
    {
        if (!hd[idx] && ptr[idx - 8] < ptr[idx])
        {
            ptr[idx] = ptr[idx - 8];
            indexes[idx] = indexes[idx - 8];
        }
        hd[idx] = hd[idx - 8] | hd[idx];
    }
    if (lane >= 16)
    {
        if (!hd[idx] && ptr[idx - 16] < ptr[idx])
        {
            ptr[idx] = ptr[idx - 16];
            indexes[idx] = indexes[idx - 16];
        }
        hd[idx] = hd[idx - 16] | hd[idx];
    }
    return ptr[idx];
}

__device__ float segscan_min_block(volatile float* ptr, volatile int* hd, volatile int* indexes)
{
    const unsigned int idx = threadIdx.x;
    unsigned int warpid = idx >> 5;
    unsigned int warp_first = warpid << 5;
    unsigned int warp_last = warp_first + 31;
    // Step 1a:
    // Before overwriting the input head flags , record whether
    // this warp begins with an "open" segment.
    bool warp_is_open = (hd[warp_first] == 0);
    __syncthreads();
    // Step 1b:
    // Intra -warp segmented scan in each warp.
    float val = segscan_min_warp(ptr, hd, indexes);
    // Step 2a:
    // Since ptr [] contains *inclusive * results , irrespective of Kind ,
    // the last value is the correct partial result.
    float warp_total = ptr[warp_last];
    // Step 2b:
    // warp_flag is the OR -reduction of the flags in a warp and is
    // computed indirectly from the mindex values in hd [].
    // will_accumulate indicates that a thread will only accumulate a
    // partial result in Step 4 if there is no segment boundary to its left.
    bool warp_flag = hd[warp_last] != 0 || !warp_is_open;
    bool will_accumulate = warp_is_open && hd[idx] == 0;
    __syncthreads();
    // Step 2c: The last thread in each warp writes partial results
    if (idx == warp_last)
    {
        ptr[warpid] = warp_total;
        hd[warpid] = warp_flag;
        indexes[warpid] = indexes[warp_last];
    }
    __syncthreads();
    // Step 3: One warp scans the per -warp results
    if (warpid == 0)
        segscan_min_warp(ptr, hd, indexes);
    __syncthreads();
    // Step 4: Accumulate results from Step 3, as appropriate .
    if (warpid != 0 && will_accumulate && ptr[warpid - 1] < val)
    {
        val = ptr[warpid - 1];
        indexes[idx] = indexes[warpid - 1];
    }
    __syncthreads();
    ptr[idx] = val;
    __syncthreads();
    return val;
}

//__device__ void radixFragmentSort()


//__device__ void segmentScan(int* arr, int *shm_indexes)
//{
//    // up-sweep phase
//    int k = 2 * threadIdx.x, d;
//    for (d = 2; d < 2 * blockDim.x; d <<= 1)
//    {
//        
//        if (k & (~((d << 1) - 1))) // k % 2*d == 0
//        {
//            if (!IS_NEW_SEGMENT(shm_indexes[k + (d << 1) - 1]))
//                arr[k + (d << 1) - 1] += arr[k + d - 1];
//            scene.tree.flags[shm_indexes[k + (d << 1) - 1]] |= IS_NEW_SEGMENT(shm_indexes[k + d - 1]);
//        }
//        __syncthreads();
//    }
//    // down-sweep phase
//    if (threadIdx.x == blockDim.x - 1) arr[2 * blockDim.x - 1] = 0;
//    __syncthreads();
//    for (d >>= 1; d >= 0; d >>= 1)
//    {
//        if (k & (~((d << 1) - 1))) // k % 2*d == 0
//        {
//            int t = arr[k + d - 1];
//            arr[k + d - 1] = arr[k + (d << 1) - 1];
//            if (IS_NEW_SEGMENT(shm_indexes[k + d]))
//                arr[k + (d << 1) - 1] = 0;
//            else if (IS_NEW_SEGMENT(shm_indexes[k + d - 1]))
//                arr[k + (d << 1) - 1] = t;
//            else
//                arr[k + (d << 1) - 1] += t;
//            scene.tree.flags[shm_indexes[k + d - 1]] &= ~0b100;
//        }
//        __syncthreads();
//    }
//}
//
//__device__ void segmentScanByIntersectionCounts(int* arr, int *shm_indexes)
//{
//    // zamień N na 1, pozostałe 0
//    // up-sweep phase
//    int k = 2 * threadIdx.x, d;
//    if (arr[k] == GET_INTERSECTION_COUNT(shm_indexes[k])) arr[k] = 1;
//    else arr[k] = 0;
//    if (arr[k + blockDim.x] == GET_INTERSECTION_COUNT(shm_indexes[k + blockDim.x])) arr[k + blockDim.x] = 1;
//    else arr[k + blockDim.x] = 0;
//    for (d = 2; d < 2 * blockDim.x; d <<= 1)
//    {
//        if (k & (~((d << 1) - 1))) // k % 2*d == 0
//        {
//            if (!IS_NEW_SEGMENT(shm_indexes[k + (d << 1) - 1]))
//                arr[k + (d << 1) - 1] += arr[k + d - 1];
//            scene.tree.flags[shm_indexes[k + (d << 1) - 1]] |= IS_NEW_SEGMENT(shm_indexes[k + d - 1]);
//        }
//        __syncthreads();
//    }
//    // down-sweep phase
//    if (threadIdx.x == blockDim.x - 1) arr[2 * blockDim.x - 1] = 0;
//    __syncthreads();
//    for (d >>= 1; d >= 0; d >>= 1)
//    {
//        if (k & (~((d << 1) - 1))) // k % 2*d == 0
//        {
//            int t = arr[k + d - 1];
//            arr[k + d - 1] = arr[k + (d << 1) - 1];
//            if (IS_NEW_SEGMENT(shm_indexes[k + d]))
//                arr[k + (d << 1) - 1] = 0;
//            else if (IS_NEW_SEGMENT(shm_indexes[k + d - 1]))
//                arr[k + (d << 1) - 1] = t;
//            else
//                arr[k + (d << 1) - 1] += t;
//            scene.tree.flags[shm_indexes[k + d - 1]] &= ~0b100;
//        }
//        __syncthreads();
//    }
//}

__device__ void findIntersectionWithRay(int x, int y, int i, float* out_distances, int* out_indexes, float* shm_faces, int* shm_indexes, int* shm_intersectionIds, int* shm_a_to_scan)
{


    

    //if (shm_a_to_scan[idx + (idx / scene.tree.n)*scene.tree.n] == 1 && (idx % scene.tree.n == 0 || shm_a_to_scan[idx + (idx / scene.tree.n) * scene.tree.n - 1] == 0)) // uwaga na illegal access!!!!!!!
    //{
    //    // mamy wynik dla piksela (x,y)

    //    

    //    out_distances[x + y * scene.width] = shm_faces[idx + (idx / scene.tree.n) * scene.tree.n];
    //    out_indexes[x + y * scene.width] = shm_indexes[idx + (idx / scene.tree.n) * scene.tree.n] * (GET_OP_ID(scene.tree.flags[shm_indexes[idx + (idx / scene.tree.n) * scene.tree.n]]) - 1);

    //    //printf("Jest wynik: %d %d %f %d\n", x, y, shm_faces[idx + (idx / scene.tree.n) * scene.tree.n], out_indexes[x+y*scene.width]);
    //}
    //if (shm_a_to_scan[idx + (idx / scene.tree.n + 1) * scene.tree.n] == 1 && shm_a_to_scan[idx + (idx / scene.tree.n + 1) * scene.tree.n - 1] == 0)
    //{
    //    // mamy wynik dla piksela (x,y)

    //    

    //    out_distances[x + y * scene.width] = shm_faces[idx + (idx / scene.tree.n + 1) * scene.tree.n];
    //    out_indexes[x + y * scene.width] = shm_indexes[idx + (idx / scene.tree.n + 1) * scene.tree.n] * (GET_OP_ID(scene.tree.flags[shm_indexes[idx + (idx / scene.tree.n + 1) * scene.tree.n]]) - 1);

    //    //printf("Jest wynik: %d %d %f %d\n", x, y, shm_faces[idx + (idx / scene.tree.n + 1) * scene.tree.n], out_indexes[x+y*scene.width]);
    //}

    
    
    //unitNormal = make_float3(
    //    normalSign * (scene.observer.x - scene.tree.x[index] + vecTopointOnScene.x * distance) / scene.tree.radius[index],
    //    normalSign * (scene.observer.y - scene.tree.y[index] + vecTopointOnScene.y * distance) / scene.tree.radius[index],
    //    normalSign * (scene.observer.z - scene.tree.z[index] + vecTopointOnScene.z * distance) / scene.tree.radius[index]);
    //prodNL = scalarProduct(unitNormal, scene.unitVectorToLight);
    //unitReflected = normalised(make_float3(
    //    2 * prodNL * unitNormal.x - scene.unitVectorToLight.x,
    //    2 * prodNL * unitNormal.y - scene.unitVectorToLight.y,
    //    2 * prodNL * unitNormal.z - scene.unitVectorToLight.z));
    //prodVR = scalarProduct(unitReflected, unitToScene);

    //color = scene.tree.color[index];

    //return distance;
}

__global__ void calculateDistancesAndIndexes(float *out_distances, int* out_indexes)
{
    //extern __shared__ float data[];

    __shared__ float shm_faces[2*1024];
    __shared__ int shm_intersectionIds[2*1024];
    __shared__ int shm_indexes[2*1024];
    __shared__ int shm_a_to_scan[2*1024];
    //__shared__ int shm_perm[2 * 1024];
    
    const int index = threadIdx.x + blockIdx.x * blockDim.x;
    const unsigned int idx = threadIdx.x, idxPlusBlockDim = idx + blockDim.x;
    const int x = (index / scene.tree.n) % scene.width, y = (index / scene.tree.n) / scene.width, i = index % scene.tree.n;
    //if (threadIdx.x < scene.tree.n) // !!!!!!!!!
    //{
    //    data[threadIdx.x] = scene.tree.operationId[threadIdx.x];
    //    data[threadIdx.x + scene.tree.n] = scene.tree.operationId[threadIdx.x + scene.tree.n];
    //    data[threadIdx.x + 2 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 2*scene.tree.n];
    //    data[threadIdx.x + 3 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 3*scene.tree.n];
    //    data[threadIdx.x + 4 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 4*scene.tree.n];
    //    data[threadIdx.x + 5 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 5*scene.tree.n];
    //}
   /* scene.tree.operationId = data;
    scene.tree.color = scene.tree.operationId + scene.tree.n;
    scene.tree.x = (float*)(scene.tree.color + scene.tree.n);
    scene.tree.y = scene.tree.x + scene.tree.n;
    scene.tree.z = scene.tree.y + scene.tree.n;
    scene.tree.radius = scene.tree.z + scene.tree.n;*/

    if (y < scene.height)
    {
        //findIntersectionWithRay(x, y, i, out_distances, out_indexes, data, (int*)(data+2*blockDim.x), (int*)(data + 2 * blockDim.x) + 2 * blockDim.x, (int*)(data + 2 * blockDim.x) + 4 * blockDim.x);
        
        float3 vecTopointOnScene = make_float3(
            scene.planeDistance * scene.direction.x - (1.0f - 2.0f * x / scene.width) * scene.planeHorizontal.x * scene.planeHeight / 2 * scene.width / scene.height + (1.0f - 2.0f * y / scene.height) * scene.planeHeight / 2 * (scene.direction.y * scene.planeHorizontal.z - scene.direction.z * scene.planeHorizontal.y),
            scene.planeDistance * scene.direction.y - (1.0f - 2.0f * x / scene.width) * scene.planeHorizontal.y * scene.planeHeight / 2 * scene.width / scene.height + (1.0f - 2.0f * y / scene.height) * scene.planeHeight / 2 * (scene.direction.z * scene.planeHorizontal.x - scene.direction.x * scene.planeHorizontal.z),
            scene.planeDistance * scene.direction.z - (1.0f - 2.0f * x / scene.width) * scene.planeHorizontal.z * scene.planeHeight / 2 * scene.width / scene.height + (1.0f - 2.0f * y / scene.height) * scene.planeHeight / 2 * (scene.direction.x * scene.planeHorizontal.y - scene.direction.y * scene.planeHorizontal.x));
        //float3 unitToScene = minusNormalised(vecTopointOnScene);
        //float3 unitNormal, unitReflected;
        float distance = INFINITY;
        int index = 0, normalSign;

        //float faces[2];

        calculateIntersection(i, &shm_faces[idx], &shm_faces[idxPlusBlockDim], vecTopointOnScene);

        shm_intersectionIds[idx] = GET_INTERSECTION_ID(i);
        shm_intersectionIds[idxPlusBlockDim] = GET_INTERSECTION_ID(i);

        shm_indexes[idx] = i;// | (scene.tree.flags[i] & 0xffff0000);
        shm_indexes[idxPlusBlockDim] = i;// | (scene.tree.flags[i] & 0xffff0000);

        /*shm_perm[idx] = idx;
        shm_perm[idxPlusBlockDim] = idxPlusBlockDim;*/

        if (shm_faces[idx] == INFINITY)
        {
            shm_a_to_scan[idx] = 0;
            shm_a_to_scan[idxPlusBlockDim] = 0;
        }
        else if (GET_OP_ID(scene.tree.flags[i]) == OP_INTERSECTION)
        {
            shm_a_to_scan[idx] = 1;
            shm_a_to_scan[idxPlusBlockDim] = -1;
        }
        else
        {
            shm_a_to_scan[idx] = -1;
            shm_a_to_scan[idxPlusBlockDim] = 1;
        }

        // dodać shm_a_to_scan to poniższych funkcji sortujących!
        fragmentSortInBlock(shm_faces, shm_indexes, shm_intersectionIds, shm_a_to_scan);
        fragmentSortInBlock(shm_faces + blockDim.x, shm_indexes + blockDim.x, shm_intersectionIds + blockDim.x, shm_a_to_scan + blockDim.x);



        mergeTwoBlockSizeArrays(shm_faces, shm_indexes, shm_intersectionIds, shm_a_to_scan);


        /*  __syncthreads();
         if (x == 300 && y == 300 && i == 0)
         {
             for (int j = 0; j < 2 * blockDim.x; j++)
                 printf("%f;%d;%d;%d\n", shm_faces[j], shm_indexes[j], GET_INTERSECTION_COUNT(shm_indexes[idx]), shm_intersectionIds[j]);
         }
         __syncthreads();*/

        int setIdx = 0, setIdxPlusBlockDim = 0;
        if (idx == 0 || shm_intersectionIds[idx] > shm_intersectionIds[idx - 1])
        {
            setIdx = 1;
        }

        if (shm_intersectionIds[idxPlusBlockDim] > shm_intersectionIds[idxPlusBlockDim - 1])
        {
            setIdxPlusBlockDim = 1;
        }

        shm_intersectionIds[idx] = setIdx;
        shm_intersectionIds[idxPlusBlockDim] = setIdxPlusBlockDim;


        // wybierz najmniejsze takie i, że scan = liczba przecięć
        segscan_block(shm_a_to_scan, shm_intersectionIds);
        segscan_block(shm_a_to_scan + blockDim.x, shm_intersectionIds + blockDim.x);




        shm_intersectionIds[idx] = setIdx;
        shm_intersectionIds[idxPlusBlockDim] = setIdxPlusBlockDim;
        /*if (x == 0 && y == 0 && i == 0)
        {
            for (int j = 0; j < 2 * blockDim.x; j++)
            {
                printf("(%f,%d,%d),\t", shm_faces[j], shm_indexes[j], shm_intersectionIds[j]);
            }
            printf("\n");
        }*/


        //if (shm_a_to_scan[idx] == GET_INTERSECTION_COUNT(shm_indexes[idx])) shm_a_to_scan[idx] = 1;
        //else shm_a_to_scan[idx] = 0;

        //if (shm_a_to_scan[idxPlusBlockDim] == GET_INTERSECTION_COUNT(shm_indexes[idxPlusBlockDim])) shm_a_to_scan[idxPlusBlockDim] = 1;
        //else shm_a_to_scan[idxPlusBlockDim] = 0;



        //segscan_block(shm_a_to_scan, shm_intersectionIds);
        //segscan_block(shm_a_to_scan + blockDim.x, shm_intersectionIds + blockDim.x);




        //if (!(shm_a_to_scan[idx] == 1 && (setIdx || shm_a_to_scan[idx - 1] == 0)))
        //{
        //    shm_faces[idx] = INFINITY;
        //}
        //if (!(shm_a_to_scan[idxPlusBlockDim] == 1 && (setIdxPlusBlockDim || shm_a_to_scan[idxPlusBlockDim - 1] == 0)))
        //{
        //    shm_faces[idxPlusBlockDim] = INFINITY;
        //}
        //shm_intersectionIds[idx] = idx % (2 * scene.tree.n) == 0 ? 1 : 0;
        //shm_intersectionIds[idxPlusBlockDim] = (idxPlusBlockDim) % (2 * scene.tree.n) == 0 ? 1 : 0;


        if (shm_a_to_scan[idx] != GET_INTERSECTION_COUNT(shm_indexes[idx])) shm_faces[idx] = INFINITY;
        //else shm_a_to_scan[idx] = 0;

        if (shm_a_to_scan[idxPlusBlockDim] != GET_INTERSECTION_COUNT(shm_indexes[idxPlusBlockDim])) shm_faces[idxPlusBlockDim] = INFINITY;
        //else shm_a_to_scan[idxPlusBlockDim] = 0;

        shm_intersectionIds[idx] = idx % (2 * scene.tree.n) == 0 ? 1 : 0;
        shm_intersectionIds[idxPlusBlockDim] = (idxPlusBlockDim) % (2 * scene.tree.n) == 0 ? 1 : 0;

        segscan_min_block(shm_faces, shm_intersectionIds, shm_indexes);
        segscan_min_block(shm_faces + blockDim.x, shm_intersectionIds + blockDim.x, shm_indexes + blockDim.x);

        /* __syncthreads();
        if (x == 300 && y == 300 && i == 0)
        {
            for (int j = 0; j < 2 * blockDim.x; j++)
                printf("%f;%d;%d;%d\n", shm_faces[j], shm_indexes[j], GET_INTERSECTION_COUNT(shm_indexes[idx]), shm_intersectionIds[j]);
        }
        __syncthreads();*/

        out_distances[x + y * scene.width] = INFINITY;
        if (idx % scene.tree.n == scene.tree.n - 1)
        {
            out_distances[x + y * scene.width] = shm_faces[2 * idx + 1];
            out_indexes[x + y * scene.width] = shm_indexes[2 * idx + 1];
            //if (y == 0) printf("%d;%f\n", x, shm_faces[2 * idx + 1]);
        }
    }
}

__global__ void renderView(float* in_distances, int* in_indexes_out_pixels)
{
    //extern __shared__ float data[];
   // __shared__ float data[1024 * (2 + 6)];
    int index = threadIdx.x + blockIdx.x * blockDim.x;

    //if (threadIdx.x < scene.tree.n) // !!!!!!!!!
    //{
    //    data[threadIdx.x] = scene.tree.operationId[threadIdx.x];
    //    data[threadIdx.x + scene.tree.n] = scene.tree.operationId[threadIdx.x + scene.tree.n];
    //    data[threadIdx.x + 2 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 2*scene.tree.n];
    //    data[threadIdx.x + 3 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 3*scene.tree.n];
    //    data[threadIdx.x + 4 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 4*scene.tree.n];
    //    data[threadIdx.x + 5 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 5*scene.tree.n];
    //}
   /* scene.tree.operationId = data;
    scene.tree.color = scene.tree.operationId + scene.tree.n;
    scene.tree.x = (float*)(scene.tree.color + scene.tree.n);
    scene.tree.y = scene.tree.x + scene.tree.n;
    scene.tree.z = scene.tree.y + scene.tree.n;
    scene.tree.radius = scene.tree.z + scene.tree.n;*/

    int x = index % scene.width, y = index / scene.width;
    if (y < scene.height)
    {
        int normalSign = 1;
        if (in_indexes_out_pixels[index] < 0)
        {
            normalSign = -1;
            in_indexes_out_pixels[index] = -in_indexes_out_pixels[index];
        }
        float distance = in_distances[index], radius = scene.tree.radius[in_indexes_out_pixels[index]];
        float3 vecTopointOnScene = make_float3(
            scene.planeDistance * scene.direction.x - (1.0f - 2.0f * x / scene.width) * scene.planeHorizontal.x * scene.planeHeight / 2 * scene.width / scene.height + (1.0f - 2.0f * y / scene.height) * scene.planeHeight / 2 * (scene.direction.y * scene.planeHorizontal.z - scene.direction.z * scene.planeHorizontal.y),
            scene.planeDistance * scene.direction.y - (1.0f - 2.0f * x / scene.width) * scene.planeHorizontal.y * scene.planeHeight / 2 * scene.width / scene.height + (1.0f - 2.0f * y / scene.height) * scene.planeHeight / 2 * (scene.direction.z * scene.planeHorizontal.x - scene.direction.x * scene.planeHorizontal.z),
            scene.planeDistance * scene.direction.z - (1.0f - 2.0f * x / scene.width) * scene.planeHorizontal.z * scene.planeHeight / 2 * scene.width / scene.height + (1.0f - 2.0f * y / scene.height) * scene.planeHeight / 2 * (scene.direction.x * scene.planeHorizontal.y - scene.direction.y * scene.planeHorizontal.x));
        float3 unitToScene = minusNormalised(vecTopointOnScene);
        float3 unitNormal = make_float3(
            normalSign * (scene.observer.x - scene.tree.x[in_indexes_out_pixels[index]] + vecTopointOnScene.x * distance) / radius,
            normalSign * (scene.observer.y - scene.tree.y[in_indexes_out_pixels[index]] + vecTopointOnScene.y * distance) / radius,
            normalSign * (scene.observer.z - scene.tree.z[in_indexes_out_pixels[index]] + vecTopointOnScene.z * distance) / radius);
        float prodNL = scalarProduct(unitNormal, scene.unitVectorToLight);
        float3 unitReflected = normalised(make_float3(
            2 * prodNL * unitNormal.x - scene.unitVectorToLight.x,
            2 * prodNL * unitNormal.y - scene.unitVectorToLight.y,
            2 * prodNL * unitNormal.z - scene.unitVectorToLight.z));
        float prodVR = scalarProduct(unitReflected, unitToScene);

        int color = scene.tree.color[in_indexes_out_pixels[index]];

        if (prodNL < 0) prodNL = 0;
        if (prodVR < 0) prodVR = 0;
        prodVR = powf(prodVR, scene.light.m);
        float r = ((color & 0xff0000) / (float)0xff0000) * (scene.light.ka + scene.light.kd * prodNL) + scene.light.ks * prodVR,
            g = ((color & 0xff00) / (float)0xff00) * (scene.light.ka + scene.light.kd * prodNL) + scene.light.ks * prodVR,
            b = ((color & 0xff) / (float)0xff) * (scene.light.ka + scene.light.kd * prodNL) + scene.light.ks * prodVR;

        if (distance < INFINITY)
        {
            //printf("Koloruje %d, %d, %f, %f\n", x, y, prodNL, prodVR);
            in_indexes_out_pixels[index] = 0xff000000 | ((r > 1.0f ? 0xff : (int)(r * 0xff)) << 16) | ((g > 1.0f ? 0xff : (int)(g * 0xff)) << 8) | (b > 1.0f ? 0xff : (int)(b * 0xff));
        }
        else in_indexes_out_pixels[index] = 0xff000000;

    }
}

//__global__ void calculateDistancesAndIndexes(int* distances, int* indexes)
//{
//    extern __shared__ float data[];
//    int index = threadIdx.x + blockIdx.x * blockDim.x;
//
//    //if (threadIdx.x < scene.tree.n) // !!!!!!!!!
//    //{
//    //    data[threadIdx.x] = scene.tree.operationId[threadIdx.x];
//    //    data[threadIdx.x + scene.tree.n] = scene.tree.operationId[threadIdx.x + scene.tree.n];
//    //    data[threadIdx.x + 2 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 2*scene.tree.n];
//    //    data[threadIdx.x + 3 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 3*scene.tree.n];
//    //    data[threadIdx.x + 4 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 4*scene.tree.n];
//    //    data[threadIdx.x + 5 * scene.tree.n] = scene.tree.operationId[threadIdx.x + 5*scene.tree.n];
//    //}
//   /* scene.tree.operationId = data;
//    scene.tree.color = scene.tree.operationId + scene.tree.n;
//    scene.tree.x = (float*)(scene.tree.color + scene.tree.n);
//    scene.tree.y = scene.tree.x + scene.tree.n;
//    scene.tree.z = scene.tree.y + scene.tree.n;
//    scene.tree.radius = scene.tree.z + scene.tree.n;*/
//
//    int x = (index / scene.tree.n) % scene.width, y = (index / scene.tree.n) / scene.width, i = index % scene.tree.n;
//    if (y < scene.height)
//    {
//        float prodNL, prodVR;
//        int color;
//        float distance = findIntersectionWithRay(x, y, i, prodNL, prodVR, color, data, (int*)(data + 2 * blockDim.x), (int*)(data + 2 * blockDim.x) + 2 * blockDim.x, (int*)(data + 2 * blockDim.x) + 4 * blockDim.x);
//        if (prodNL < 0) prodNL = 0;
//        if (prodVR < 0) prodVR = 0;
//        prodVR = powf(prodVR, scene.light.m);
//        float r = ((color & 0xff0000) / (float)0xff0000) * (scene.light.ka + scene.light.kd * prodNL) + scene.light.ks * prodVR,
//            g = ((color & 0xff00) / (float)0xff00) * (scene.light.ka + scene.light.kd * prodNL) + scene.light.ks * prodVR,
//            b = ((color & 0xff) / (float)0xff) * (scene.light.ka + scene.light.kd * prodNL) + scene.light.ks * prodVR;
//
//        if (distance < INFINITY)
//        {
//            printf("Koloruje %d, %d, %f, %f\n", x, y, prodNL, prodVR);
//            out[index] = 0xff000000 | ((r > 1.0f ? 0xff : (int)(r * 0xff)) << 16) | ((g > 1.0f ? 0xff : (int)(g * 0xff)) << 8) | (b > 1.0f ? 0xff : (int)(b * 0xff));
//        }
//        else out[index] = 0xff000000;
//    }
//}

void allocAndCreateTree(normalisedCsgTree& tree, int **pd_data, int *flag_data, int *color_data, float *shape_data, int n)
{
    tree.n = n;
    cudaError_t err = cudaMalloc(pd_data, TREE_SIZE(tree));
    if (err != cudaSuccess)
    {
        printf("%s\n", cudaGetErrorString(err));
    }
    cudaMemcpy(*pd_data, flag_data, sizeof(int) * tree.n, cudaMemcpyHostToDevice);
    cudaMemcpy(*pd_data + tree.n, color_data, sizeof(int) * tree.n, cudaMemcpyHostToDevice);
    cudaMemcpy(*pd_data + 2 * tree.n, shape_data, 4 * sizeof(float) * tree.n, cudaMemcpyHostToDevice);
    tree.flags = *pd_data;
    tree.color = tree.flags + tree.n;
    tree.x = (float*)(tree.color + tree.n);
    tree.y = tree.x + tree.n;
    tree.z = tree.y + tree.n;
    tree.radius = tree.z + tree.n;
}

extern "C" void __declspec(dllexport) __stdcall GPURender
(
    int h_out[],
    int width,
    int height,
    float cameraParams[9],
    float planeDistance,
    float planeHeight,
    float lightParams[6],
    //int tree_shapeIds[],
    int tree_flags[],
    int color_data[],
    float raw_spheres[],
    int treeSize
)
{
    struct sceneParameters sc{};
    sc.observer = make_float3(cameraParams[0], cameraParams[1], cameraParams[2]);
    sc.direction = make_float3(cameraParams[3], cameraParams[4], cameraParams[5]);
    sc.planeHorizontal = make_float3(cameraParams[6], cameraParams[7], cameraParams[8]);
    sc.width = width;
    sc.height = height;
    sc.planeDistance = planeDistance;
    sc.planeHeight = planeHeight;
    sc.unitVectorToLight = make_float3(cos(lightParams[1]) * cos(lightParams[0]), cos(lightParams[1]) * sin(lightParams[0]), -sin(lightParams[1]));
    sc.light.ka = lightParams[2];
    sc.light.kd = lightParams[3];
    sc.light.ks = lightParams[4];
    sc.light.m = lightParams[5];
    
    int* d_out, *d_tdata;
    float* d_dist;
    allocAndCreateTree(sc.tree, &d_tdata, tree_flags, color_data, raw_spheres, treeSize);
    cudaMalloc(&d_out, width * height * sizeof(int));
    cudaMalloc(&d_dist, width* height * sizeof(float));
    cudaMemset(d_out, 0, width * height * sizeof(int)); 
    cudaMemset(d_dist, 0xff, width* height * sizeof(float));
    cudaError_t err = cudaMemcpyToSymbol(scene, &sc, sizeof(struct sceneParameters));
    int blockDim = (1024 / sc.tree.n) * sc.tree.n;
    int blocks = ceil((float)width * height * sc.tree.n / blockDim);
    //int shmSize = (2 * sizeof(float) + 6 * sizeof(int)) * blockDim; //TREE_SIZE(sc.tree);
    calculateDistancesAndIndexes<<<blocks, blockDim/*, shmSize*/>>>(d_dist, d_out);
    blockDim = 1024;
    blocks = ceil((float)width * height / blockDim);
    renderView<<<blocks, blockDim>>>(d_dist, d_out);
    cudaMemcpy(h_out, d_out, width * height * sizeof(int), cudaMemcpyDeviceToHost);
    cudaFree(d_out);
    cudaFree(d_tdata);
    cudaFree(d_dist);
}

//__global__ void calculateIntersection2(int offset, float* faces, float* backFace)
//{
//    const int index = offset + threadIdx.x + blockDim.x * blockIdx.x;
//    int xIm = (index / scene.tree.n) % scene.width, yIm = (index / scene.tree.n) / scene.width, i = index % scene.tree.n;
//    float x = scene.tree.x[i], y = scene.tree.y[i], z = scene.tree.z[i], radius = scene.tree.radius[i];
//
//    float3 vecTopointOnScene = make_float3(
//        scene.planeDistance * scene.direction.x - (1.0f - 2.0f * xIm / scene.width) * scene.planeHorizontal.x * scene.planeHeight / 2 * scene.width / scene.height + (1.0f - 2.0f * yIm / scene.height) * scene.planeHeight / 2 * (scene.direction.y * scene.planeHorizontal.z - scene.direction.z * scene.planeHorizontal.y),
//        scene.planeDistance * scene.direction.y - (1.0f - 2.0f * xIm / scene.width) * scene.planeHorizontal.y * scene.planeHeight / 2 * scene.width / scene.height + (1.0f - 2.0f * yIm / scene.height) * scene.planeHeight / 2 * (scene.direction.z * scene.planeHorizontal.x - scene.direction.x * scene.planeHorizontal.z),
//        scene.planeDistance * scene.direction.z - (1.0f - 2.0f * xIm / scene.width) * scene.planeHorizontal.z * scene.planeHeight / 2 * scene.width / scene.height + (1.0f - 2.0f * yIm / scene.height) * scene.planeHeight / 2 * (scene.direction.x * scene.planeHorizontal.y - scene.direction.y * scene.planeHorizontal.x));
//
//    //http://paulbourke.net/geometry/circlesphere/index.html#linesphere
//    float a = vecTopointOnScene.x * vecTopointOnScene.x + vecTopointOnScene.y * vecTopointOnScene.y + vecTopointOnScene.z * vecTopointOnScene.z,
//        b = 2 * (vecTopointOnScene.x * (scene.observer.x - x) + vecTopointOnScene.y * (scene.observer.y - y) + vecTopointOnScene.z * (scene.observer.z - z)),
//        c = x * x + y * y + z * z + scene.observer.x * scene.observer.x + scene.observer.y * scene.observer.y + scene.observer.z * scene.observer.z - 2 * (x * scene.observer.x + y * scene.observer.y + z * scene.observer.z) - radius * radius;
//
//    float delta = b * b - 4 * a * c;
//    if (delta >= 0)
//    {
//        faces[2*index] = (-b - sqrt(delta)) / 2 / a;
//        faces[2 * index+1] = (-b + sqrt(delta)) / 2 / a;
//    }
//    else
//    {
//        faces[2 * index] = INFINITY;
//        faces[2 * index+1] = -INFINITY;
//    }
//}
//
//extern "C" void __declspec(dllexport) __stdcall GPUThrustRender
//(
//    int h_out[],
//    int width,
//    int height,
//    float cameraParams[9],
//    float planeDistance,
//    float planeHeight,
//    float lightParams[6],
//    //int tree_shapeIds[],
//    int tree_flags[],
//    int color_data[],
//    float raw_spheres[],
//    int treeSize
//)
//{
//    const long long maxMemory = 1 << 30;
//
//    struct sceneParameters sc {};
//    sc.observer = make_float3(cameraParams[0], cameraParams[1], cameraParams[2]);
//    sc.direction = make_float3(cameraParams[3], cameraParams[4], cameraParams[5]);
//    sc.planeHorizontal = make_float3(cameraParams[6], cameraParams[7], cameraParams[8]);
//    sc.width = width;
//    sc.height = height;
//    sc.planeDistance = planeDistance;
//    sc.planeHeight = planeHeight;
//    sc.unitVectorToLight = make_float3(cos(lightParams[1]) * cos(lightParams[0]), cos(lightParams[1]) * sin(lightParams[0]), -sin(lightParams[1]));
//    sc.light.ka = lightParams[2];
//    sc.light.kd = lightParams[3];
//    sc.light.ks = lightParams[4];
//    sc.light.m = lightParams[5];
//
//    long long requiredMemory = width * height * sc.tree.n;
//
//    int* d_out, * d_tdata;
//    float* d_dist;
//    allocAndCreateTree(sc.tree, &d_tdata, tree_flags, color_data, raw_spheres, treeSize);
//    cudaMalloc(&d_out, width * height * sizeof(int));
//    cudaMalloc(&d_dist, width * height * sizeof(float));
//    cudaMemset(d_out, 0, width * height * sizeof(int));
//    cudaMemset(d_dist, 0xff, width * height * sizeof(float));
//    cudaError_t err = cudaMemcpyToSymbol(scene, &sc, sizeof(struct sceneParameters));
//    int blockDim = (1024 / sc.tree.n) * sc.tree.n;
//    int blocks = ceil((float)width * height * sc.tree.n / blockDim);
//    int shmSize = (2 * sizeof(float) + 6 * sizeof(int)) * blockDim; //TREE_SIZE(sc.tree);
//    calculateDistancesAndIndexes << <blocks, blockDim, shmSize >> > (d_dist, d_out);
//    blockDim = 1024;
//    blocks = ceil((float)width * height / blockDim);
//    renderView << <blocks, blockDim >> > (d_dist, d_out);
//    cudaMemcpy(h_out, d_out, width * height * sizeof(int), cudaMemcpyDeviceToHost);
//    cudaFree(d_out);
//    cudaFree(d_tdata);
//    cudaFree(d_dist);
//}


int main()
{
    int width = 1000, height = 1000;
    /*int tree_flags[]{ MAKE_FLAG(0,2,1,1), MAKE_FLAG(0,2,0,1) };
    float raw_spheres[]{ 0, 0.5f, 0, 0, 1.5f, 1.5f, 0.5f, 0.5f };*/
    int tree_flags[]{ 5,1,65541,65537 };
    float raw_spheres[]{ 0.5,0.25,0,0.25,0,0.3,0,0.3,1.5,1.5,1.5,1.5,0.5,0.5,0.5,0.5 };
    float cameraParams[]{ 0,0,0,0,0,1,0,1,0 };
    int color_data[]{ 0xffffffff,0xffff0000, 0xffffffff, 0xffffffff };
    float lightParams[]{ 0,0,0.1,0.45,0.45,30 };
    int* h_out;
    cudaHostAlloc(&h_out, width * height * sizeof(int), cudaHostAllocDefault);
    GPURender(h_out, width, height, cameraParams, 0.5, 0.5, lightParams, tree_flags, color_data, raw_spheres, 4);
    cudaFreeHost(h_out);
    cudaError_t err = cudaGetLastError();  // add
    if (err != cudaSuccess) printf("CUDA error: %s", cudaGetErrorString(err)); // add
    return 0;
}

