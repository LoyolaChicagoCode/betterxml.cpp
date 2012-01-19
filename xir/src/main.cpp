#include <cstdio>
#include <iostream>
#include "expat-wrapper.h"

using namespace std;


int main(int argc, char *argv[]) {
    FILE *in_file;

    if(argc > 1) {
	in_file = fopen(argv[1], "r");
    } else {
	in_file = stdin;
    }

    XMLParser *parser = new XMLParser();
    parser->InitParser(NULL);
    parser->Parse(in_file);
    delete parser;

    //add in close
}

