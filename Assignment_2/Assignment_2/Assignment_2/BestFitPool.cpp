#include "BestFitPool.h"



BestFitPool::BestFitPool(unsigned int poolSize) : MemoryPool(poolSize){

}
//
//std::vector<MemoryPool::Chunk>::iterator BestFitPool::FindAvailableChunk(unsigned int nbytes) {
//
//	// Implementing Best Fit Algorithm
//	// Examine all free chunks, find the one that is closest in size to what we want
//	// Start by finding the first available chunk that big enough.
//	auto theOne = chunks.begin();
//	while (theOne != chunks.end() && !(!theOne->allocated && theOne->size >= nbytes))
//	{
//		theOne++;
//	}
//
//	// TODO: test for out of memory?
//
//	// ... continue by comparing every other available chunk to find a better one
//	for (auto iter = std::next(theOne); iter != chunks.end(); iter++)
//	{
//		if (!iter->allocated && iter->size >= nbytes && iter->size < theOne->size) {
//			theOne = iter;
//		}
//	}
//
//	return theOne;
//}