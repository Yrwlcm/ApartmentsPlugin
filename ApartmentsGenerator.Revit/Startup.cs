using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ApartmentsGenerator.Revit;

[Transaction(TransactionMode.Manual)]
public class Startup : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        throw new NotImplementedException();
    }
}