// Assignment #2 - CST-352 - Spring 2020
// By Kate LaFrance

#include <iostream>
#include "FirstFitPool.h"

int main()
{
    FirstFitPool ffp(100);
    ffp.DebugPrint();
    void* c1 = ffp.Allocate(10);
    ffp.DebugPrint();
    ffp.Free(c1);
    ffp.DebugPrint();
    std::cout << "Done!\n";
}
