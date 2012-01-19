CXX=g++
LIBS= -lexpat 
xmlapp:
	$(CXX) -c -o base64.o base64.cpp
	$(CXX) -c -o xdo.o xdo.cpp 
	$(CXX) -c -o expat-wrapper.o expat-wrapper.cpp $(LIBS)
	$(CXX) -c -o main.o main.cpp 
	$(CXX) -o xml2xir main.o expat-wrapper.o xdo.o base64.o $(LIBS)
clean:
	rm *.o
	rm xml2xir
