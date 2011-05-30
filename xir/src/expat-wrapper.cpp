/* 
 * 
 * Expat wrapping from:
 * EasySoap++ - A C++ library for SOAP (Simple Object Access Protocol)
 * Copyright (C) 2001 David Crowley; SciTegic, Inc.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public
 * License as published by the Free Software Foundation; either
 * version 2 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 * License along with this library; if not, write to the Free
 * Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 * ns not working on the singletons
*/
#include <iostream>
#include <expat.h>
#include <string>
#include <sstream>
#include <utility>
#include "xdo.h"
#include "expat-wrapper.h"

#define EXPAT_VER(a,b,c) (((((a)*1000)+(b))*1000)+(c))
#define EXPAT_VERSION EXPAT_VER(XML_MAJOR_VERSION,XML_MINOR_VERSION,XML_MICRO_VERSION)

using namespace std;

pair<string, string> getNamespaceDataPair(string value) {
    int loc = value.find('#', 0); //character shouldn't be hardcoded in
    int size = value.size();

    if(loc>0) {
	return pair<string, string>(value.substr(0, loc), value.substr(loc+1, size));
    } else {
	return pair<string, string>(string("None"), value);
    }
}

XMLParser::XMLParser() {
  m_parser = 0;
}

XMLParser::~XMLParser() {
  FreeParser();
}

void
XMLParser::FreeParser() {
  if (m_parser) {
      XML_ParserFree(m_parser);
      m_parser = 0;
    }
}

void
XMLParser::InitParser(const char *encoding) {
  if (m_parser) {
      XML_ParserReset(m_parser, encoding);
  } else {
      FreeParser();
      m_parser = (struct XML_ParserStruct*)XML_ParserCreateNS(encoding, '#');
  }

  XML_SetElementHandler(m_parser, XMLParser::_startElement, XMLParser::_endElement);
  XML_SetCharacterDataHandler(m_parser, XMLParser::_characterData);
  XML_SetStartNamespaceDeclHandler(m_parser, XMLParser::_startNamespace);
  XML_SetEndNamespaceDeclHandler(m_parser, XMLParser::_endNamespace);
  XML_SetProcessingInstructionHandler(m_parser, XMLParser::_processInstruction);
  //this doesn't seem to do anything:
  XML_SetUserData(m_parser, this);
}

void *
XMLParser::GetParseBuffer(int size) {
  if (m_parser)
    return XML_GetBuffer(m_parser, size);
  return 0;
}

bool
XMLParser::ParseBuffer(int size) {
  if (m_parser)
    return XML_ParseBuffer(m_parser, size, size == 0) != 0;
  return false;
}

int
XMLParser::Parse(FILE *file) {
  startDocument();
  char buf[BUFSIZ];
  int done;
  int depth = 0;
  do {
    size_t len = fread(buf, 1, sizeof(buf), file);
    done = len < sizeof(buf);
    if (XML_Parse(m_parser, buf, len, done) == XML_STATUS_ERROR) {
      cout << "Error!\n";
      return 1;
    }
  } while (!done);
  endDocument();
}

const char *
XMLParser::GetErrorMessage() {
  if (m_parser)
    return XML_ErrorString(XML_GetErrorCode(m_parser));
  return 0;
}

void 
XMLParser::startDocument() {
  XIRDataObject xir("document", "begin");
  xir.print();
}

void 
XMLParser::endDocument() {
  XIRDataObject xir("document", "end");
  xir.print();
}

void
XMLParser::startElement(const char *name, const char **attributes) {
  int i;
  XIRDataObject xir("element", "begin");
  for(i=0;attributes[i];i+=2) { /*remove this duplicate loop*/ }
  stringstream str_stream;
  string str;
  str_stream << i/2;
  str_stream >> str;
  xir.setVerbatim("attributes", str.c_str());

  string name_str(name);
  pair<string, string> ns_data_pair = getNamespaceDataPair(name_str);
  xir.setVerbatim("ns", ns_data_pair.first);
  xir.setVerbatim("name", ns_data_pair.second);  
  xir.print();

  //loop over attributes and print them out
  for(i=0;attributes[i];i+=2) {
      string attribute(attributes[i]);
      string value(attributes[i+1]);

      //add check in here to see if we're talking about element xmlns?

      XIRDataObject attrObject("a", "singleton");
      pair<string, string> ns_data_pair_attr = getNamespaceDataPair(attribute);
      attrObject.setVerbatim("ns", ns_data_pair_attr.first.compare("None") ? ns_data_pair_attr.first : ns_data_pair.first);
      attrObject.setVerbatim("name", ns_data_pair_attr.second);  
      attrObject.setVerbatim("value", value);
   
      attrObject.print();    
  }
}

void
XMLParser::endElement(const char *name) {
  string name_str(name);

  XIRDataObject xir("element", "end");
  pair<string, string> ns_data_pair = getNamespaceDataPair(name);
  xir.setVerbatim("ns", ns_data_pair.first);
  xir.setVerbatim("name", ns_data_pair.second);  

  xir.print();
}

void
XMLParser::characterData(const char *data, int len) {
  string data_str(data, len);
  XIRDataObject xir("characters", NULL);
  xir.setBase64("cdata", data_str.c_str());
  xir.print();
}

void 
XMLParser::processInstruction(const char *target, const char *data) { 
    XIRDataObject xir("pi", NULL);
    xir.setVerbatim("name", data); //Is this right??
    xir.setVerbatim("target", target);
}

void
XMLParser::startNamespace(const char *prefix, const char *uri) { 
    XIRDataObject xir("prefix", "begin");
    string prefix_string(prefix ? prefix : "None");
    string uri_string(uri ? uri : "None" );
    xir.setVerbatim("prefix", prefix_string);
    xir.setVerbatim("uri", uri_string);
    xir.print();
    uri_map[prefix_string] = uri_string;
}

void
XMLParser::endNamespace(const char *prefix) { 
    string prefix_string(prefix ? prefix : "None");
    XIRDataObject xir("prefix", "end");
    xir.setVerbatim("prefix", prefix);
    xir.setVerbatim("uri", uri_map[prefix_string]);
    xir.print();
    uri_map.erase(prefix_string);
}


// static methods for compatibility with the C expat calls
void
XMLParser::_startElement(void *userData, const XML_Char *name, const XML_Char **attrs) {
  XMLParser *parser = (XMLParser *) userData;
  parser->startElement(name, attrs);
}

void
XMLParser::_endElement(void *userData, const XML_Char *name) {
  XMLParser *parser = (XMLParser *) userData;
  parser->endElement(name);
}

void
XMLParser::_characterData(void *userData, const XML_Char *str, int len) {
  XMLParser *parser = (XMLParser *) userData;
  parser->characterData(str, len);
}

void
XMLParser::_startNamespace(void *userData, const XML_Char *prefix, const XML_Char *uri) {
  XMLParser *parser = (XMLParser *) userData;
  parser->startNamespace(prefix, uri);
}

void
XMLParser::_endNamespace(void *userData, const XML_Char *prefix) {
  XMLParser *parser = (XMLParser *) userData;
  parser->endNamespace(prefix);
}

void 
XMLParser::_processInstruction(void *userData, const XML_Char *target, const XML_Char *data) {
  XMLParser *parser = (XMLParser *) userData;
  parser->processInstruction(target, data);
}




