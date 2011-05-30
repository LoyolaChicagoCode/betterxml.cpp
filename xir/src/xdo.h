#include <map>
#include <utility>
#include <string>

const static int BASE64 = 64;
const static int VERBATIM = 65;

using namespace std;

class XIRDataObject {
public:
  XIRDataObject(const char *type, const char *subtype);
  virtual ~XIRDataObject();
  void setVerbatim(const char *key, const char *value);
  void setVerbatim(const char *key, string value);
  void setBase64(const char *key, const char *value);
  void print();

private:
  map<string, pair<int, string> > elements;
};
