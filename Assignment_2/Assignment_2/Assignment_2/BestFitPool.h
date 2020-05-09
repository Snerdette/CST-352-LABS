#pragma once
#include "MemoryPool.h"
class BestFitPool : public MemoryPool
{
public:
	BestFitPool(unsigned int poolSize);

	//virtual void* Allocate(unsigned int nbytes);

protected:
	virtual std::vector<Chunk>::iterator FindAvailableChunk(unsigned int nbytes);

};

