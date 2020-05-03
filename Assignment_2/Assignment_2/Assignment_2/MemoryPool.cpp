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
	if (nbytes > poolSize)
		throw OutOfMemoryException();

	auto iter = chunks.begin();
	while (iter != chunks.end() && !(!iter->allocated && iter->size >= nbytes)) 
	{
		iter++;
	}

	if (iter ==  chunks.end())
		throw OutOfMemoryException();
	
	chunks.emplace(iter, Chunk(iter->startingIndex, nbytes, true));
	iter->size -= nbytes;
	iter->startingIndex += nbytes;

	return pool + iter->startingIndex;
}

void MemoryPool::Free(void* block) 
{

}

void MemoryPool::DebugPrint() 
{
	std::cout << "MemoryPool {" << std::endl;
	for (auto iter = chunks.begin(); iter !=chunks.end(); iter++)
	{
		std::cout << "\t" << iter->startingIndex << ", " << iter->size << ", " << iter->allocated << std::endl;
	}
	std::cout << "}" << std::endl;
}