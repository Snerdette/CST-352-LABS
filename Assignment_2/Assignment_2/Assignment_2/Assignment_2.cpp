// Assignment #2 - CST-352 - Spring 2020
// By Kate LaFrance

#include <iostream>
#include "FirstFitPool.h"
#include "BestFitPool.h"

void test1(MemoryPool& pool) 
{
    pool.DebugPrint();
    void* c1 = pool.Allocate(10);
    pool.DebugPrint();
    void* c2 = pool.Allocate(10);
    pool.DebugPrint();
    void* c3 = pool.Allocate(80);
    pool.DebugPrint();
    pool.Free(c1);
    pool.DebugPrint();
    pool.Free(c3);
    pool.DebugPrint();
    pool.Free(c2);
    pool.DebugPrint();
    std::cout << "Done!\n";
}

int main()
{
    FirstFitPool ffp(100);
    std::cout << "First Fit Pool test1: \n";
    test1(ffp);

    BestFitPool bfp(100);
    std::cout << "Best Fit Pool test1: \n";
    test1(bfp);


    std::cout << "Done!\n";
}
