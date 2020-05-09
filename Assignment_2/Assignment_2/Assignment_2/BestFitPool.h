#pragma once
#include "MemoryPool.h"
class BestFitPool : public MemoryPool
{
public:
	BestFitPool(unsigned int poolSize);

	virtual void* Allocate(unsigned int nbytes);

};

