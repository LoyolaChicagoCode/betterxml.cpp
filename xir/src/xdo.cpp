#include <iostream>
#include <map>
#include <string>
#include <utility>
#include "base64.h"
#include "xdo.h"

using namespace std;

XIRDataObject::XIRDataObject(const char *type, const char *subtype) {
  XIRDataObject::setVerbatim("xir.type", type);
  XIRDataObject::setVerbatim("xir.subtype", subtype ? subtype : "unused");
}

XIRDataObject::~XIRDataObject() { }

void 
XIRDataObject::setVerbatim(const char *key, string value) {
  string key_string(key);
  pair<int, string> p(VERBATIM, value);
  elements[key_string] = p;
}

void 
XIRDataObject::setVerbatim(const char *key, const char *value) {
    setVerbatim(key, value ? string(value) : string("None"));
}
  
void 
XIRDataObject::setBase64(const char *key, const char *value) {
  string key_string(key);
  string value_string(value); //add base64 conversion
  string encoded = base64_encode(reinterpret_cast<const unsigned char*>(value_string.c_str()), value_string.length());
  pair<int, string> p(BASE64, encoded);
  elements[key_string] = p;
}

char *getTypeString(int type) {
  if(type==BASE64) {
    return "base64=";
  } else if(type==VERBATIM) {
    return "verbatim=";
  }
}

void 
XIRDataObject::print() {
  map<string, pair<int, string> >::const_iterator iter;

  // I think there's a way to filter the iterator with a lambda expressiony-function-thing
  for (iter=elements.begin(); iter != elements.end(); ++iter) {
    if(!iter->first.find_first_of("xir.")) {
	cout << iter->first << ":" << getTypeString(iter->second.first) << iter->second.second << endl;
    }
  }

  for (iter=elements.begin(); iter != elements.end(); ++iter) {
    if(iter->first.find_first_of("xir.")) {
	cout << iter->first << ":" << getTypeString(iter->second.first) << iter->second.second << endl;
    }
  }

  cout << endl;
}


