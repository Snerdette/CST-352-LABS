#include "FirstFitPool.h"


FirstFitPool::FirstFitPool(unsigned int poolSize) : MemoryPool(poolSize)
{

}

//void* FirstFitPool::Allocate(unsigned int nbytes)
//{
//	// Allocate  chunk of memory from the pool, nbytes big
//	// If the pool is not big enough for nbytes, throw OutOfMemoryException
//	if (nbytes > poolSize)
//		throw OutOfMemoryException();
//
//	// Find an available chunk that has a size at least as large as nbytes
//	// HERE:
//	// Implementing the First Fit Algorithm:
//	// Find the first chunk that is not allocatedd and is at least nbytes big
//	auto iter = chunks.begin();
//	while (iter != chunks.end() && !(!iter->allocated && iter->size >= nbytes))
//	{
//		iter++;
//	}
//
//	// If we can't find one, throw OutOfMemoryException
//	if (iter == chunks.end())
//		throw OutOfMemoryException();
//	if (iter->size == nbytes) {
//		// chunk is exactly the right size
//		// Allocate it!
//		iter->allocated = true;
//		// Return a pointer ti the allocated memory referenced by the chunk
//		return pool + iter->startingIndex;
//	}
//	else {
//		// chunk is bigger than needed
//		// Split into two chunks, one that is nbytes (allocated) and the other being the remainder (available)
//		// Fix up the free chunk to be smaller and start after the freshly allocated chunk.
//		auto saveStartingIndex = iter->startingIndex;
//		iter->size -= nbytes;
//		iter->startingIndex += nbytes;
//		// Insert a new chunk before the free chunk, that is exactly nbytes big and it's allocated
//		auto newIter = chunks.emplace(iter, Chunk(saveStartingIndex, nbytes, true));
//
//
//		// Return a pointer to the allocated emmory referenced by the new chunk
//		return pool + saveStartingIndex;
//	}
//
//}


std::vector<MemoryPool::Chunk>::iterator FirstFitPool::FindAvailableChunk(unsigned int nbytes)
{
	// Implementing the First Fit Algorithm:
	// Find the first chunk that is not allocatedd and is at least nbytes big
	auto iter = chunks.begin();
	while (iter != chunks.end() && !(!iter->allocated && iter->size >= nbytes))
	{
		iter++;
	}
	return iter;
}