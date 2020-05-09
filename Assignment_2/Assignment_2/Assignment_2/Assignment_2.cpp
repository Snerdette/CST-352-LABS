// Assignment #2 - CST-352 - Spring 2020
// By Kate LaFrance

#include <iostream>
#include "FirstFitPool.h"
#include "BestFitPool.h"

void test1(MemoryPool& pool) 
{
    std::cout << "test2: \n";
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
    std::cout << "Done with test1!\n";
}

void test2(MemoryPool& pool)
{
    std::cout << "test2: \n";
    void* c1 = pool.Allocate(10);
    void* c2 = pool.Allocate(81);
    pool.Free(c1);
    pool.DebugPrint();

    // At this point, there should be two free blocks... 10 and 9
    void* c3 = pool.Allocate(8);
    pool.DebugPrint();
    std::cout << "Done with test2!\n";
}

int main()
{
    FirstFitPool ffp(100);
    std::cout << "First Fit Pool test2: \n";
    test2(ffp);

    BestFitPool bfp(100);
    std::cout << "Best Fit Pool test2: \n";
    test2(bfp);


    std::cout << "Done!\n";
}
