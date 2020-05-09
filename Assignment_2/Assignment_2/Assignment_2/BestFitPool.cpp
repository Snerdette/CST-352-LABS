#include "BestFitPool.h"



BestFitPool::BestFitPool(unsigned int poolSize) : MemoryPool(poolSize){

}


//void* BestFitPool::Allocate(unsigned int nbytes)
//{
//	// Allocate  chunk of memory from the pool, nbytes big
//	// If the pool is not big enough for nbytes, throw OutOfMemoryException
//	if (nbytes > poolSize)
//		throw OutOfMemoryException();
//
//	// Find an available chunk that has a size at least as large as nbytes
//	// HERE:
//	// Implementing the Best Fit Algorithm
//	// Examine all free chunks, find one that is closest in size to what we want
//	// Start by finding the first availb;e chunk that's big enough...
//	auto theOne = chunks.begin();
//	while (theOne != chunks.end() && !(!theOne->allocated && theOne->size >= nbytes))
//	{
//		theOne++;
//	}
//
//	// Continue by comparing every other available chunk to find a better one
//	for (auto iter = std::next(theOne); iter != chunks.end(); iter++)
//	{
//		// iter better than theOne?
//		if (!iter->allocated && iter->size >= nbytes && iter->size < theOne->size)
//		{
//			theOne = iter;
//		}
//	}
//
//
//
//	// If we can't find one, throw OutOfMemoryException
//	if (theOne == chunks.end())
//		throw OutOfMemoryException();
//	if (theOne->size == nbytes) {
//		// chunk is exactly the right size
//		// Allocate it!
//		theOne->allocated = true;
//		// Return a pointer ti the allocated memory referenced by the chunk
//		return pool + theOne->startingIndex;
//	}
//	else {
//		// chunk is bigger than needed
//		// Split into two chunks, one that is nbytes (allocated) and the other being the remainder (available)
//		// Fix up the free chunk to be smaller and start after the freshly allocated chunk.
//		auto saveStartingIndex = theOne->startingIndex;
//		theOne->size -= nbytes;
//		theOne->startingIndex += nbytes;
//		// Insert a new chunk before the free chunk, that is exactly nbytes big and it's allocated
//		auto newIter = chunks.emplace(theOne, Chunk(saveStartingIndex, nbytes, true));
//
//
//		// Return a pointer to the allocated emmory referenced by the new chunk
//		return pool + saveStartingIndex;
//	}
//
//}

std::vector<MemoryPool::Chunk>::iterator BestFitPool::FindAvailableChunk(unsigned int nbytes)
{
	 // Implementing Best Fit Algorithm
	// Examine all free chunks, find the one that is closest in size to what we want
	// Start by finding the first available chunk that big enough.
	auto theOne = chunks.begin();
	while (theOne != chunks.end() && !(!theOne->allocated && theOne->size >= nbytes))
	{
		theOne++;
	}

	// ... continue by comparing every other available chunk to find a better one
	for (auto iter = std::next(theOne); iter != chunks.end(); iter++)
	{
		if (!iter->allocated && iter->size >= nbytes && iter->size < theOne->size) {
			theOne = iter;
		}
	}

	return theOne;
}