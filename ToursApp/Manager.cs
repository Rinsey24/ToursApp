using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ToursApp.Models;
namespace ToursApp;

    
class Manager
{
    public static Frame MainFrame { get; set; }
    public static ToursContext DbContext => ToursContext.GetInstance();
}
