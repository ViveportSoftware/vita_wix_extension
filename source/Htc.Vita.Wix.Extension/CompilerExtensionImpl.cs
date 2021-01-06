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
                        case "BootTimeFetcher":
                            ParseBootTimeFetcherElement(element);
                            break;
                        case "CurrentTimestampFetcher":
                            ParseCurrentTimestampFetcherElement(element);
                            break;
                        case "RegistryKeyCleaner":
                            ParseRegistryKeyCleanerElement(element);
                            break;
                        case "RegistryValueCleaner":
                            ParseRegistryValueCleanerElement(element);
                            break;
                        case "ServiceManager":
                            ParseServiceManagerElement(element);
                            break;
                        case "SidTranslator":
                            ParseSidTranslatorElement(element);
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

        private void ParseBootTimeFetcherElement(XmlElement element)
        {
            var sourceLineNumber = Preprocessor.GetSourceLineNumbers(element);
            string id = null;
            string valuePropertyId = null;
            var asUtc = YesNoType.No;

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
                        case "Value":
                            valuePropertyId = Core.GetAttributeValue(
                                    sourceLineNumber,
                                    attribute
                            );
                            break;
                        case "AsUtc":
                            asUtc = Core.GetAttributeYesNoValue(
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
                var row = Core.CreateRow(
                        sourceLineNumber,
                        "VitaBootTimeFetcher"
                );
                row[0] = id;
                row[1] = valuePropertyId;
                row[2] = asUtc == YesNoType.Yes ? 1 : 0;
            }

            Core.CreateWixSimpleReferenceRow(
                    sourceLineNumber,
                    "CustomAction",
                    "Vita_BootTimeFetcher"
            );
        }

        private void ParseCurrentTimestampFetcherElement(XmlElement element)
        {
            var sourceLineNumber = Preprocessor.GetSourceLineNumbers(element);
            string id = null;
            string format = null;
            string valuePropertyId = null;
            var asUtc = YesNoType.No;

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
                        case "Format":
                            format = Core.GetAttributeValue(
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
                        case "AsUtc":
                            asUtc = Core.GetAttributeYesNoValue(
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

            if (string.IsNullOrEmpty(format))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "Format"
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
                var row = Core.CreateRow(
                        sourceLineNumber,
                        "VitaCurrentTimestampFetcher"
                );
                row[0] = id;
                row[1] = format;
                row[2] = valuePropertyId;
                row[3] = asUtc == YesNoType.Yes ? 1 : 0;
            }

            Core.CreateWixSimpleReferenceRow(
                    sourceLineNumber,
                    "CustomAction",
                    "Vita_CurrentTimestampFetcher"
            );
        }

        private void ParseRegistryKeyCleanerElement(XmlElement element)
        {
            var sourceLineNumber = Preprocessor.GetSourceLineNumbers(element);
            string id = null;
            string keyScope = null;
            string keyPath = null;

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
                        case "Scope":
                            keyScope = Core.GetAttributeValue(
                                    sourceLineNumber,
                                    attribute
                            );
                            break;
                        case "Path":
                            keyPath = Core.GetAttributeValue(
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

            if (string.IsNullOrEmpty(keyScope))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "Scope"
                ));
            }

            if (string.IsNullOrEmpty(keyPath))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "Path"
                ));
            }

            if (!Core.EncounteredError)
            {
                var row = Core.CreateRow(
                        sourceLineNumber,
                        "VitaRegistryKeyCleaner"
                );
                row[0] = id;
                row[1] = keyScope;
                row[2] = keyPath;
            }

            Core.CreateWixSimpleReferenceRow(
                    sourceLineNumber,
                    "CustomAction",
                    "Vita_RegistryKeyCleanerImmediate"
            );
        }

        private void ParseRegistryValueCleanerElement(XmlElement element)
        {
            var sourceLineNumber = Preprocessor.GetSourceLineNumbers(element);
            string id = null;
            string keyScope = null;
            string keyPath = null;
            string keyName = null;

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
                        case "Scope":
                            keyScope = Core.GetAttributeValue(
                                    sourceLineNumber,
                                    attribute
                            );
                            break;
                        case "Path":
                            keyPath = Core.GetAttributeValue(
                                    sourceLineNumber,
                                    attribute
                            );
                            break;
                        case "Name":
                            keyName = Core.GetAttributeValue(
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

            if (string.IsNullOrEmpty(keyScope))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "Scope"
                ));
            }

            if (string.IsNullOrEmpty(keyPath))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "Path"
                ));
            }

            if (string.IsNullOrEmpty(keyName))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "Name"
                ));
            }

            if (!Core.EncounteredError)
            {
                var row = Core.CreateRow(
                        sourceLineNumber,
                        "VitaRegistryValueCleaner"
                );
                row[0] = id;
                row[1] = keyScope;
                row[2] = keyPath;
                row[3] = keyName;
            }

            Core.CreateWixSimpleReferenceRow(
                    sourceLineNumber,
                    "CustomAction",
                    "Vita_RegistryValueCleanerImmediate"
            );
        }

        private void ParseServiceManagerElement(XmlElement element)
        {
            var sourceLineNumber = Preprocessor.GetSourceLineNumbers(element);
            string id = null;
            string serviceName = null;
            string serviceStartType = null;

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
                        case "Name":
                            serviceName = Core.GetAttributeValue(
                                    sourceLineNumber,
                                    attribute
                            );
                            break;
                        case "StartType":
                            serviceStartType = Core.GetAttributeValue(
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

            if (string.IsNullOrEmpty(serviceName))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "Name"
                ));
            }

            if (string.IsNullOrEmpty(serviceStartType))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(
                        sourceLineNumber,
                        element.Name,
                        "StartType"
                ));
            }

            if (!Core.EncounteredError)
            {
                var row = Core.CreateRow(
                        sourceLineNumber,
                        "VitaServiceManager"
                );
                row[0] = serviceName;
                row[1] = serviceStartType;
            }

            Core.CreateWixSimpleReferenceRow(
                    sourceLineNumber,
                    "CustomAction",
                    "Vita_ServiceManagerImmediate"
            );
        }

        private void ParseSidTranslatorElement(XmlElement element)
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
                var row = Core.CreateRow(
                        sourceLineNumber,
                        "VitaSidTranslator"
                );
                row[0] = sidKey;
                row[1] = valuePropertyId;
            }

            Core.CreateWixSimpleReferenceRow(
                    sourceLineNumber,
                    "CustomAction",
                    "Vita_SidTranslator"
            );
        }
    }
}
