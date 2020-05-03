#pragma once
// Assignment #2 - CST-352 - Spring 2020
// By Kate LaFrance
class MemoryPool 
{
protected:

	MemoryPool(unsigned int poolSize);

public:

	virtual void* Allocate(unsigned int nbytes);

	virtual void Free(void* block);

	virtual void DebugPrint();
};