using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace DynamicXmlLib
{
    public class DynamicXml : DynamicObject, IEnumerable
    {
        private readonly List<XElement> _elements;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">Provide the path of the xml file</param>
        public DynamicXml(XDocument doc)
        {
            _elements = new List<XElement> { doc.Root };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">Provide and xml document as string</param>
        public DynamicXml(string text)
        {
            var doc = XDocument.Parse(text);
            _elements = new List<XElement> { doc.Root };
        }


        protected DynamicXml(XElement element)
        {
            _elements = new List<XElement> { element };
        }

        protected DynamicXml(IEnumerable<XElement> elements)
        {
            _elements = new List<XElement>(elements);
        }


        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (binder.Name == "Value")   //Get the value of the element       
                result = _elements.Select(x => x.Value).FirstOrDefault();
            else if (binder.Name == "Count")
                result = _elements.Count;
            else
            {

                var attr = _elements[0].Attribute(XName.Get(binder.Name));    //Get the attribute that matches the binder name.  
                if (attr != null)
                {
                    result = attr.Value; //Get the attribute value. 
                }
                else
                {
                    var items = _elements.Descendants().Where(x => x.Name.LocalName == binder.Name); //Get all of the child elements in the document that match the binder name.
                    if (items != null || items.Count() > 0)
                    {
                        result = new DynamicXml(items);

                    }
                }
            }

            //if (result == null)
            //{
            //    if (_elements.Count() == 0)
            //        _elements.Add(new XElement(binder.Name));
            //    else
            //        _elements.First().AddFirst(new XElement(binder.Name));

            //    if (_elements[0].HasElements)
            //        result = new DynamicXml(_elements[0].Descendants().First());
            //}
            // else 
            if (result != null)
            {
                var res = result as DynamicXml;
                //Check if the current element has children. If it does, return the dynamic 
                if (res != null && res._elements.Count > 0)
                    if (res._elements.First().HasElements)
                        result = new DynamicXml(res._elements[0].Descendants());
            }
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder.Name == "Value")
            {
                /* the Value property is the only one that may be modified.
                TryGetMember actually creates new XML elements in this implementation */
                _elements[0].Value = value.ToString();
                return true;
            }
            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            int ndx = (int)indexes[0];
            result = new DynamicXml(_elements[ndx]);
            return true;
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var element in _elements)
                yield return new DynamicXml(element);
        }
    }
}
