using System.Reflection;
using Microsoft.Tools.WindowsInstallerXml;

namespace Htc.Vita.Wix.Extension
{
    public class WixExtensionImpl : WixExtension
    {
        private CompilerExtension _compilerExtension;

        public override CompilerExtension CompilerExtension => _compilerExtension ?? (
                _compilerExtension = new CompilerExtensionImpl()
        );

        private TableDefinitionCollection _tableDefinitions;

        public override TableDefinitionCollection TableDefinitions => _tableDefinitions ?? (
                _tableDefinitions = LoadTableDefinitionHelper(
                        Assembly.GetExecutingAssembly(),
                        "Htc.Vita.Wix.Extension.TableDefinitions.xml"
                )
        );

        private Library _library;

        public override Library GetLibrary(TableDefinitionCollection tableDefinitions)
        {
            return _library ?? (
                    _library = LoadLibraryHelper(
                            Assembly.GetExecutingAssembly(),
                            "Htc.Vita.Wix.Extension.Htc.Vita.Wix.CustomLibrary.wixlib",
                            tableDefinitions
                    )
            );
        }
    }
}
