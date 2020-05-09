#include <iostream>
#include <string>
#include <iterator>
#include <vector>
#include "MemoryPool.h"


MemoryPool::MemoryPool(unsigned int poolSize) :
poolSize(poolSize)
{
	// Allocate the pool of bytes
	pool = new unsigned char[poolSize];

	// Create 1 big available chunk covering the pool
	chunks.emplace_back(Chunk(0, poolSize, false));
}

void* MemoryPool::Allocate(unsigned int nbytes) 
{
	/*
	// Allocate  chunk of memory from the pool, nbytes big
	// If the pool is not big enough for nbytes, throw OutOfMemoryException
	if (nbytes > poolSize)
		throw OutOfMemoryException();

	// Find an available chunk that has a size at least as large as nbytes
	// For now, we're implementing First Fit Algorithm
	// Find the first chunk that is not allocatedd and is at least nbytes big
	auto iter = chunks.begin();
	while (iter != chunks.end() && !(!iter->allocated && iter->size >= nbytes)) 
	{
		iter++;
	}

	// If we can't find one, throw OutOfMemoryException
	if (iter ==  chunks.end())
		throw OutOfMemoryException();
	if (iter->size == nbytes) {
		// chunk is exactly the right size
		// Allocate it!
		iter->allocated = true;
		// Return a pointer ti the allocated memory referenced by the chunk
		return pool + iter->startingIndex;
	}
	else {
		// chunk is bigger than needed
		// Split into two chunks, one that is nbytes (allocated) and the other being the remainder (available)
		// Fix up the free chunk to be smaller and start after the freshly allocated chunk.
		auto saveStartingIndex = iter->startingIndex;
		iter->size -= nbytes;
		iter->startingIndex += nbytes;
		// Insert a new chunk before the free chunk, that is exactly nbytes big and it's allocated
		auto newIter = chunks.emplace(iter, Chunk(saveStartingIndex, nbytes, true));


		// Return a pointer to the allocated emmory referenced by the new chunk
		return pool + saveStartingIndex;
	}
	*/
	return nullptr;
}

void MemoryPool::Free(void* block) 
{
	// Free up the memory previously allocated for the block

	// Find the chunk for this block
	auto chunk = chunks.begin();
	while (chunk != chunks.end())
	{
		if (block == (pool + chunk->startingIndex)) 
		{
			// We found it!
			// Mark as not alloceted
			chunk->allocated = false;

			// Find other free chunk before abd after and combine them into one free chunk

			// Try the previous chunk, if there is one
			if (chunk->startingIndex > 0) 
			{
				// Move back the previous chunk
				auto prevChunk = std::prev(chunk);

				// Assimilate it, if it's free
				if (!prevChunk->allocated) 
				{
					chunk->size += prevChunk->size;
					chunk->startingIndex = prevChunk->startingIndex;
					chunk = chunks.erase(prevChunk);
				}
				
			}

			if (chunk->startingIndex + chunk->size < poolSize)
			{
				// get the next chunk
				auto nextChunk = std::next(chunk);

				// Assimilate it, if it's free
				if (!nextChunk->allocated) 
				{
					chunk->size += nextChunk->size;
					chunk = chunks.erase(nextChunk);
				}
				

			}
			
			// We're done!
			return;
		}
		// Next chunk
		chunk++;
	}
}

void MemoryPool::DebugPrint() 
{
	std::cout << "MemoryPool {" << std::endl;
	for (auto& iter : chunks)
	{
		std::cout << "\t" << iter.startingIndex << ", " << iter.size << ", " << iter.allocated << std::endl;
	}
	std::cout << "}" << std::endl;
}