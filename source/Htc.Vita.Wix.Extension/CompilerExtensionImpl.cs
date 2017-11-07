using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Tools.WindowsInstallerXml;

namespace Htc.Vita.Wix.Extension
{
    public class CompilerExtensionImpl : CompilerExtension
    {
        public override XmlSchema Schema { get; }

        public CompilerExtensionImpl()
        {
            Schema = LoadXmlSchemaHelper(
                    Assembly.GetExecutingAssembly(),
                    "Htc.Vita.Wix.Extension.Schema.xsd"
            );
        }

        public override void ParseElement(
                SourceLineNumberCollection sourceLineNumbers,
                XmlElement parentElement,
                XmlElement element,
                params string[] contextValues)
        {
            switch (parentElement.LocalName)
            {
                case "Product":
                case "Fragment":
                    switch (element.LocalName)
                    {
                        case "SidTranslate":
                            ParseSidTranslateElement(element);
                            break;
                        default:
                            Core.UnexpectedElement(
                                    parentElement,
                                    element
                            );
                            break;
                    }
                    break;
                default:
                    Core.UnexpectedElement(
                            parentElement,
                            element
                    );
                    break;
            }
        }

        private void ParseSidTranslateElement(XmlElement element)
        {
            var sourceLineNumber = Preprocessor.GetSourceLineNumbers(element);
            string id = null;
            string sidKey = null;
            string valuePropertyId = null;

            foreach (XmlAttribute attribute in element.Attributes)
            {
                if (attribute.NamespaceURI.Length == 0 ||
                        attribute.NamespaceURI == Schema.TargetNamespace)
                {
                    switch (attribute.LocalName)
                    {
                        case "Id":
                            id = Core.GetAttributeIdentifierValue(
                                    sourceLineNumber,
                                    attribute
                            );
                            break;
                        case "Key":
                            sidKey = Core.GetAttributeValue(
                                    sourceLineNumber,
                                    attribute
                            );
                            break;
                        case "Value":
                            valuePropertyId = Core.GetAttributeValue(
                                    sourceLineNumber,
                                    attribute
                            );
                            break;

                        default:
                            Core.UnexpectedAttribute(
                                    sourceLineNumber,
                                    attribute
                            );
                            break;
                    }
                }
                else
                {
                    Core.UnsupportedExtensionAttribute(
                            sourceLineNumber,
                            attribute
                    );
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "Id"
                ));
            }

            if (string.IsNullOrEmpty(sidKey))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "Key"
                ));
            }

            if (string.IsNullOrEmpty(valuePropertyId))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "Value"
                ));
            }

            if (!Core.EncounteredError)
            {
                var sidTranslateRow = Core.CreateRow(
                        sourceLineNumber,
                        "VitaSidTranslate"
                );
                sidTranslateRow[0] = sidKey;
                sidTranslateRow[1] = valuePropertyId;
            }

            Core.CreateWixSimpleReferenceRow(
                    sourceLineNumber,
                    "CustomAction",
                    "Vita_SidTranslate"
            );
        }
    }
}
