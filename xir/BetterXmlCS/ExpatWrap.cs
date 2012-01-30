using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;


namespace BetterXml
{
    internal class ExpatWrap : IDisposable
    {
        #region Lib Expat Const define

        private const char NullChar = (char)0;
        private const char Colon = ':';
        private const char Equal = '=';

        public const char NSSep = '\x1F';      // ASCII US (unit separator)
        public const char ContextSep = '\x0C'; // ASCII DC2 (device control 2)
        public const int DefaultReadBufSize = 16384;

        public enum XMLBool : byte
        {
            FALSE = 0,
            TRUE = 1
        }

        public enum XMLStatus : int
        {
            ERROR = 0,
            OK,
            SUSPENDED
        };

        /**<summary>Represents XML_Error enum in Expat library.</summary>
 * <remarks>Must be in sync with <see cref="XMLErrorSet"/>.</remarks>
 */
        public enum XMLError : int
        {
            NONE,
            NO_MEMORY,
            SYNTAX,
            NO_ELEMENTS,
            INVALID_TOKEN,
            UNCLOSED_TOKEN,
            PARTIAL_CHAR,
            TAG_MISMATCH,
            DUPLICATE_ATTRIBUTE,
            JUNK_AFTER_DOC_ELEMENT,
            PARAM_ENTITY_REF,
            UNDEFINED_ENTITY,
            RECURSIVE_ENTITY_REF,
            ASYNC_ENTITY,
            BAD_CHAR_REF,
            BINARY_ENTITY_REF,
            ATTRIBUTE_EXTERNAL_ENTITY_REF,
            MISPLACED_XML_PI,
            UNKNOWN_ENCODING,
            INCORRECT_ENCODING,
            UNCLOSED_CDATA_SECTION,
            EXTERNAL_ENTITY_HANDLING,
            NOT_STANDALONE,
            UNEXPECTED_STATE,
            ENTITY_DECLARED_IN_PE,
            FEATURE_REQUIRES_XML_DTD,
            CANT_CHANGE_FEATURE_ONCE_PARSING,
            UNBOUND_PREFIX,
            UNDECLARING_PREFIX,
            INCOMPLETE_PE,
            XML_DECL,
            TEXT_DECL,
            PUBLICID,
            SUSPENDED,
            NOT_SUSPENDED,
            ABORTED,
            FINISHED,
            SUSPEND_PE,
            RESERVED_PREFIX_XML,
            RESERVED_PREFIX_XMLNS,
            RESERVED_NAMESPACE_URI

        };

        #endregion

        #region LibExpat Native code

#if MONO
        public const string expatLib = "expatw";
#else
        public const string expatLib = "libexpatw.dll";
#endif


        #region call back type in Expat Library


        /**<summary>Represents XML_StartElementHandler call-back type in Expat library.</summary> */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void
        XMLStartElementHandler(IntPtr userData, char* name, char** atts);

        /**<summary>Represents XML_EndElementHandler call-back type in Expat library.</summary> */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void XMLEndElementHandler(IntPtr userData, char* name);

        /**<summary>Represents XML_CharacterDataHandler call-back type in Expat library.</summary> */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void XMLCharacterDataHandler(IntPtr userData, char* s, int len);

        /**<summary>Represents XML_ProcessingInstructionHandler call-back type in Expat library.</summary> */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void XMLProcessingInstructionHandler(IntPtr userData, char* target, char* data);

        /**<summary>Represents XML_StartNamespaceDeclHandler call-back type in Expat library.</summary> */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void XMLStartNamespaceDeclHandler(IntPtr userData, char* prefix, char* uri);

        /**<summary>Represents XML_EndNamespaceDeclHandler call-back type in Expat library.</summary> */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void XMLEndNamespaceDeclHandler(IntPtr userData, char* prefix);

        #endregion

        [DllImport(expatLib,
                   EntryPoint = "XML_ParserFree",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void XMLParserFree(IntPtr parser);

        [DllImport(expatLib,
                  EntryPoint = "XML_ParserCreateNS",
                  CharSet = CharSet.Unicode,
                  CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr XMLParserCreateNS(string encoding, char namespaceSeparator);

        [DllImport(expatLib,
                   EntryPoint = "XML_SetReturnNSTriplet",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void XMLSetReturnNSTriplet(IntPtr parser, int returnNSTriplet);

        [DllImport(expatLib,
                  EntryPoint = "XML_Parse",
                  CharSet = CharSet.Unicode,
                  CallingConvention = CallingConvention.Cdecl)]
        public static extern XMLStatus XMLParse(IntPtr parser, [In, MarshalAs(UnmanagedType.LPArray)]                 byte[] s, int len, int isFinal);

        [DllImport(expatLib,
                   EntryPoint = "XML_ParserReset",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern XMLBool XMLParserReset(IntPtr parser, string encoding);

        [DllImport(expatLib,
                          EntryPoint = "XML_SetElementHandler",
                          CharSet = CharSet.Unicode,
                          CallingConvention = CallingConvention.Cdecl)]
        public static extern void XMLSetElementHandler(IntPtr parser, XMLStartElementHandler start, XMLEndElementHandler end);

        [DllImport(expatLib,
                 EntryPoint = "XML_SetStartElementHandler",
                 CharSet = CharSet.Unicode,
                 CallingConvention = CallingConvention.Cdecl)]
        public static extern void XMLSetStartElementHandler(IntPtr parser, XMLStartElementHandler handler);

        [DllImport(expatLib,
                   EntryPoint = "XML_SetEndElementHandler",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void XMLSetEndElementHandler(IntPtr parser, XMLEndElementHandler handler);

        [DllImport(expatLib,
                   EntryPoint = "XML_SetCharacterDataHandler",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void XMLSetCharacterDataHandler(IntPtr parser, XMLCharacterDataHandler handler);

        [DllImport(expatLib,
                   EntryPoint = "XML_SetProcessingInstructionHandler",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void XMLSetProcessingInstructionHandler(IntPtr parser, XMLProcessingInstructionHandler handler);


        [DllImport(expatLib,
                  EntryPoint = "XML_SetNamespaceDeclHandler",
                  CharSet = CharSet.Unicode,
                  CallingConvention = CallingConvention.Cdecl)]
        public static extern void
        XMLSetNamespaceDeclHandler(IntPtr parser, XMLStartNamespaceDeclHandler start, XMLEndNamespaceDeclHandler end);

        [DllImport(expatLib,
                   EntryPoint = "XML_SetStartNamespaceDeclHandler",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void
        XMLSetStartNamespaceDeclHandler(IntPtr parser, XMLStartNamespaceDeclHandler start);

        [DllImport(expatLib,
                   EntryPoint = "XML_SetEndNamespaceDeclHandler",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void
        XMLSetEndNamespaceDeclHandler(IntPtr parser, XMLEndNamespaceDeclHandler end);


        [DllImport(expatLib,
                   EntryPoint = "XML_SetUserData",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void XMLSetUserData(IntPtr parser, IntPtr userData);

        [DllImport(expatLib,
                   EntryPoint = "XML_GetBuffer",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern byte* XMLGetBuffer(IntPtr parser, int len);


        [DllImport(expatLib,
                   EntryPoint = "XML_ParseBuffer",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern XMLStatus XMLParseBuffer(IntPtr parser, int len, int isFinal);


        [DllImport(expatLib,
                   EntryPoint = "XML_GetErrorCode",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern XMLError XMLGetErrorCode(IntPtr parser);

        [DllImport(expatLib,
                   EntryPoint = "XML_ErrorString",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr _XMLErrorString(XMLError code);

        public static string XMLErrorString(XMLError code)
        {
            IntPtr errPtr = _XMLErrorString(code);
            return Marshal.PtrToStringUni(errPtr);
        }

        #endregion

        #region Event Attribute

        /* StartElementHandler */
        private XMLStartElementHandler startElementHandler;

        /// <summary>Handler for start tag.</summary>
        public XMLStartElementHandler StartElementHandler
        {
            get
            {
                return startElementHandler;
            }
            set
            {
                CheckNotDisposed();
                startElementHandler = value;
                XMLSetStartElementHandler(XmlParser, value);
            }
        }

        /* EndElementHandler */
        private XMLEndElementHandler endElementHandler;

        /// <summary>Handler for end tag.</summary>
        public XMLEndElementHandler EndElementHandler
        {
            get
            {
                return endElementHandler;
            }
            set
            {
                CheckNotDisposed();
                endElementHandler = value;
                XMLSetEndElementHandler(XmlParser, value);
            }
        }

        /* CharacterDataHandler */
        private XMLCharacterDataHandler characterDataHandler;

        /// <summary>Handler for character data.</summary>
        public XMLCharacterDataHandler CharacterDataHandler
        {
            get
            {
                return characterDataHandler;
            }
            set
            {
                CheckNotDisposed();
                characterDataHandler = value;
                XMLSetCharacterDataHandler(XmlParser, value);
            }
        }

        /* ProcessingInstructionHandler */
        private XMLProcessingInstructionHandler processingInstructionHandler;

        /// <summary>Handler for processing instructions.</summary>
        public XMLProcessingInstructionHandler ProcessingInstructionHandler
        {
            get
            {
                return processingInstructionHandler;
            }
            set
            {
                CheckNotDisposed();
                processingInstructionHandler = value;
                XMLSetProcessingInstructionHandler(XmlParser, value);
            }
        }


        /* StartNamespaceDeclHandler */
        private XMLStartNamespaceDeclHandler startNamespaceDeclHandler;

        /// <summary>Handler called when namespace scope starts.</summary>
        public XMLStartNamespaceDeclHandler StartNamespaceDeclHandler
        {
            get
            {
                return startNamespaceDeclHandler;
            }
            set
            {
                CheckNotDisposed();
                startNamespaceDeclHandler = value;
                XMLSetStartNamespaceDeclHandler(XmlParser, value);
            }
        }

        /* EndNamespaceDeclHandler */
        private XMLEndNamespaceDeclHandler endNamespaceDeclHandler;

        /// <summary>Handler called when namespace scope ends.</summary>
        public XMLEndNamespaceDeclHandler EndNamespaceDeclHandler
        {
            get
            {
                return endNamespaceDeclHandler;
            }
            set
            {
                CheckNotDisposed();
                endNamespaceDeclHandler = value;
                XMLSetEndNamespaceDeclHandler(XmlParser, value);
            }
        }

        #endregion

        #region Member variables

        private IntPtr XmlParser;
        private GCHandle ParserHandle;
        private bool IsDisposed = false;
        private bool NoError = true;
        private Dictionary<string, string> UriMap = new Dictionary<string, string>();
        #endregion

        public ExpatWrap()
        {
            XmlParser = IntPtr.Zero;
        }

        public void Dispose()
        {
            FreeParser();
            Cleanup();
            // resources cleaned up - no need to have object finalized
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        private void CheckNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void FreeParser()
        {
            if (XmlParser != IntPtr.Zero)
            {
                XMLParserFree(XmlParser);
                XmlParser = IntPtr.Zero;
            }
        }

        private void Cleanup()
        {
            if (ParserHandle.IsAllocated)
            {
                ParserHandle.Free();
            }
        }

        unsafe public void InitParser(string encoding)
        {
            if (XmlParser != IntPtr.Zero)
            {
                XMLParserReset(XmlParser, encoding);
            }
            else
            {
                FreeParser();
                XmlParser = XMLParserCreateNS(encoding, NSSep);

                StartElementHandler = OnStartElementEvent;
                EndElementHandler = OnEndElementEvent;
                CharacterDataHandler = OnCharacterEvent;
                StartNamespaceDeclHandler = OnStartPrefixMappingEvent;
                EndNamespaceDeclHandler = OnEndElementEvent;
            }

            XMLSetElementHandler(XmlParser, StartElementHandler, EndElementHandler);
            XMLSetCharacterDataHandler(XmlParser, CharacterDataHandler);
            XMLSetStartNamespaceDeclHandler(XmlParser, StartNamespaceDeclHandler);
            XMLSetEndNamespaceDeclHandler(XmlParser, EndNamespaceDeclHandler);
            XMLSetProcessingInstructionHandler(XmlParser, ProcessingInstructionHandler);

            ParserHandle = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            XMLSetUserData(XmlParser, (IntPtr)ParserHandle);
        }

        public bool Parse(Stream stream)
        {
            const int BUFFERSIZE = 1024;

            StartDocument();

            int read = 0;
            int done = 0;
            do
            {
                byte[] buf = new byte[BUFFERSIZE];

                int count = stream.Read(buf, read, BUFFERSIZE);
                read += count;
                done = count == BUFFERSIZE ? 0 : 1;
                if (XMLParse(XmlParser, buf, count, done) == XMLStatus.ERROR)
                {
                    Console.Error.WriteLine("Error!");
                    return false;
                }
            } while (done == 0);

            EndDocument();

            return NoError;
        }

        private string GetErrorMessage()
        {
            return (XmlParser == IntPtr.Zero) ? string.Empty : XMLErrorString(XMLGetErrorCode(XmlParser));
        }

        #region Util

        private static unsafe string PointToString(char* str, int length)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < length; ++i)
            {
                sb.Append(*str);
                str++;
            }

            return sb.ToString();
        }

        private static unsafe string PointToString(char* str)
        {
            StringBuilder sb = new StringBuilder();

            while (*str != NSSep && *str != NullChar)
            {
                sb.Append(*str);
                str++;
                
            }

            return sb.ToString();
        }

        private static unsafe string[] PointToStringArray(char** strs)
        {
            List<string> list = new List<string>();

            while (*strs != null && **strs != NullChar)
            {
                list.Add(PointToString(*strs));
                strs++;
            }

            return list.ToArray();
        }

        private static KeyValuePair<string, string> GetNamespaceDataPair(string value)
        {
            int loc = value.IndexOf(NSSep, 0); //character shouldn't be hardcoded in, NSSep

            if (loc > 0)
            {
                return new KeyValuePair<string, string>(value.Substring(0, loc), value.Substring(loc + 1));
            }
            else
            {
                return new KeyValuePair<string, string>("None", value);
            }
        }

        #endregion

        #region Implement Org.System.Xml.Sax.IContentHandler

        private void StartDocument()
        {
            (new XIRDataObject("document", "begin")).Print();
        }

        public void EndDocument()
        {
            (new XIRDataObject("document", "end")).Print();
        }

        private static unsafe void OnStartElementEvent(IntPtr userData, char* name, char** atts)
        {
            ExpatWrap reader = (ExpatWrap)((GCHandle)userData).Target;

            reader.StartElement(PointToString(name), PointToStringArray(atts));
        }

        public void StartElement(string name, string[] atts)
        {
            KeyValuePair<string, string> pair = GetNamespaceDataPair(name);
            XIRDataObject xir = new XIRDataObject("element", "begin");
            xir.SetVerbatim("attributes", (atts.Length / 2).ToString(CultureInfo.InvariantCulture));
            xir.SetVerbatim("ns", pair.Key);
            xir.SetVerbatim("name", pair.Value);
            xir.Print();

            //loop over attributes and print them out
            for (int i = 0; i < atts.Length - 1; i += 2)
            {
                string attribute = atts[i];
                string value = atts[i + 1];

                //add check in here to see if we're talking about element xmlns?

                KeyValuePair<string, string> dataPair = GetNamespaceDataPair(attribute);

                XIRDataObject attrObject = new XIRDataObject("a", "singleton");
                attrObject.SetVerbatim("ns", dataPair.Key == "None" ? dataPair.Key : pair.Key);
                attrObject.SetVerbatim("name", dataPair.Value);
                attrObject.SetVerbatim("value", value);
                attrObject.Print();
            }

        }

        private static unsafe void OnEndElementEvent(IntPtr userData, char* name)
        {
            ExpatWrap reader = (ExpatWrap)((GCHandle)userData).Target;

            reader.EndElement(PointToString(name));
        }

        public void EndElement(string name)
        {
            KeyValuePair<string, string> pair = GetNamespaceDataPair(name);

            XIRDataObject xir = new XIRDataObject("element", "end");
            xir.SetVerbatim("ns", pair.Key);
            xir.SetVerbatim("name", pair.Value);
            xir.Print();
        }

        private static unsafe void OnCharacterEvent(IntPtr userData, char* s, int length)
        {
            ExpatWrap reader = (ExpatWrap)((GCHandle)userData).Target;

            reader.Characters(PointToString(s, length));
        }

        public void Characters(string data)
        {
            XIRDataObject xir = new XIRDataObject("characters", null);
            xir.SetBase64("cdata", data);
            xir.Print();
        }

        private static unsafe void OnStartPrefixMappingEvent(IntPtr userData, char* prefix, char* uri)
        {
            ExpatWrap reader = (ExpatWrap)((GCHandle)userData).Target;

            reader.StartPrefixMapping(PointToString(prefix), PointToString(uri));
        }

        public void StartPrefixMapping(string prefix, string uri)
        {
            Trace.TraceInformation("[Information] StartPrefixMapping: " + prefix + " - " + uri);

            XIRDataObject xir = new XIRDataObject("prefix", "begin");
            prefix = string.IsNullOrEmpty(prefix) ? "None" : prefix;
            uri = string.IsNullOrEmpty(uri) ? "None" : uri;
            xir.SetVerbatim("prefix", prefix);
            xir.SetVerbatim("uri", uri);
            xir.Print();

            if (!UriMap.ContainsKey(prefix))
            {
                UriMap.Add(prefix, uri);
            }
        }

        private static unsafe void OnEndPrefixMappingEvent(IntPtr userData, char* prefix)
        {
            ExpatWrap reader = (ExpatWrap)((GCHandle)userData).Target;

            reader.EndPrefixMapping(PointToString(prefix));
        }

        public void EndPrefixMapping(string prefix)
        {
            Trace.TraceInformation("[Information] EndPrefixMapping: " + prefix);

            prefix = string.IsNullOrEmpty(prefix) ? "None" : prefix;
            XIRDataObject xir = new XIRDataObject("prefix", "end");
            xir.SetVerbatim("prefix", prefix);
            xir.SetVerbatim("uri", UriMap[prefix]);
            xir.Print();

            UriMap.Remove(prefix);

        }

        #endregion




    }
}
