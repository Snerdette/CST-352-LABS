#pragma once
#include <vector>
// Assignment #2 - CST-352 - Spring 2020
// By Kate LaFrance

class OutOfMemoryException{

};

class MemoryPool 
{
private:
	class Chunk 
	{
	public:
		unsigned int startingIndex;
		unsigned int size;
		bool allocated;

		Chunk(unsigned int startingIndex, unsigned int size, bool allocated) : 
			startingIndex(startingIndex), size(size), allocated(allocated){}
	};
	unsigned char* pool;
	unsigned int  poolSize;
	std::vector<Chunk> chunks;

protected:

	MemoryPool(unsigned int poolSize);

public:

	virtual void* Allocate(unsigned int nbytes);
	virtual void Free(void* block);
	virtual void DebugPrint();
};