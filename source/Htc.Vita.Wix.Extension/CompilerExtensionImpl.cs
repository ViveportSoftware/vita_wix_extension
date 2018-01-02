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

        private void ParseCurrentTimestampFetcherElement(XmlElement element)
        {
            var sourceLineNumber = Preprocessor.GetSourceLineNumbers(element);
            string id = null;
            string format = null;
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
                var currentTimestampFetcherRow = Core.CreateRow(
                        sourceLineNumber,
                        "VitaCurrentTimestampFetcher"
                );
                currentTimestampFetcherRow[0] = id;
                currentTimestampFetcherRow[1] = format;
                currentTimestampFetcherRow[2] = valuePropertyId;
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
                var registryKeyCleanerRow = Core.CreateRow(
                        sourceLineNumber,
                        "VitaRegistryKeyCleaner"
                );
                registryKeyCleanerRow[0] = id;
                registryKeyCleanerRow[1] = keyScope;
                registryKeyCleanerRow[2] = keyPath;
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
                var registryKeyCleanerRow = Core.CreateRow(
                        sourceLineNumber,
                        "VitaRegistryValueCleaner"
                );
                registryKeyCleanerRow[0] = id;
                registryKeyCleanerRow[1] = keyScope;
                registryKeyCleanerRow[2] = keyPath;
                registryKeyCleanerRow[3] = keyName;
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
                var serviceManagerRow = Core.CreateRow(
                        sourceLineNumber,
                        "VitaServiceManager"
                );
                serviceManagerRow[0] = serviceName;
                serviceManagerRow[1] = serviceStartType;
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
                var sidTranslatorRow = Core.CreateRow(
                        sourceLineNumber,
                        "VitaSidTranslator"
                );
                sidTranslatorRow[0] = sidKey;
                sidTranslatorRow[1] = valuePropertyId;
            }

            Core.CreateWixSimpleReferenceRow(
                    sourceLineNumber,
                    "CustomAction",
                    "Vita_SidTranslator"
            );
        }
    }
}
