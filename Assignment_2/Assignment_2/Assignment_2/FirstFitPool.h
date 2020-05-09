#pragma once
#include "MemoryPool.h"

// Assignment #2 - CST-352 - Spring 2020
// By Kate LaFrance

class FirstFitPool : public MemoryPool
{

public: 
	FirstFitPool(unsigned int poolSize); 
	virtual void* Allocate(unsigned int nbytes);
};

