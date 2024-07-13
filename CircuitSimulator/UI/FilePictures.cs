using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Hierarchy;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    internal static class FilePictures
    {
        public static Image FromType(string type, bool diagram, bool hideError)
        {
            Func<bool, bool, Image> func = type switch
            {
                "ac" => AC,
                "ammeter" => Ammeter,
                "battery" => Battery,
                "bell" => Bell,
                "cell" => Cell,
                "dc" => DC,
                "diode" => Diode,
                "earth" => Earth,
                "fixedresistor" => FixedResistor,
                "fuse" => Fuse,
                "generator" => Generator,
                "heater" => Heater,
                "junction" => Junction,
                "lamp" => Lamp,
                "ldr" => LDR,
                "led" => LED,
                "loudspeaker" => Loudspeaker,
                "microphone" => Microphone,
                "switch" => Switch,
                "thermistor" => Thermistor,
                "transformer" => Transformer,
                "variableresistor" => VariableResistor,
                "voltmeter" => Voltmeter,
                _ => throw new ArgumentException($"{type} was not a valid component type")
            };
            return func(diagram, hideError);
        }
        public static Image AC(bool diagram, bool hideError)
        {
            return Get("AC", diagram, hideError);
        }
        public static Image Ammeter(bool diagram, bool hideError)
        {
            return Get("Ammeter", diagram, hideError);
        }
        public static Image Battery(bool diagram, bool hideError)
        {
            return Get("Battery", diagram, hideError);
        }
        public static Image Bell(bool diagram, bool hideError)
        {
            return Get("Bell", diagram, hideError);
        }
        public static Image Cell(bool diagram, bool hideError)
        {
            return Get("Cell", diagram, hideError);
        }
        public static Image DC(bool diagram, bool hideError)
        {
            return Get("DC", diagram, hideError);
        }
        public static Image Diode(bool diagram, bool hideError)
        {
            return Get("Diode", diagram, hideError);
        }
        public static Image Earth(bool diagram, bool hideError)
        {
            return Get("Earth", diagram, hideError);
        }
        public static Image FixedResistor(bool diagram, bool hideError)
        {
            return Get("FixedResistor", diagram, hideError);
        }
        public static Image Fuse(bool diagram, bool hideError)
        {
            return Get("Fuse", diagram, hideError);
        }
        public static Image Generator(bool diagram, bool hideError)
        {
            return Get("Generator", diagram, hideError);
        }
        public static Image Heater(bool diagram, bool hideError)
        {
            return Get("Heater", diagram, hideError);
        }
        public static Image Junction(bool diagram, bool hideError)
        {
            return Get("Junction", diagram, hideError);
        }
        public static Image Lamp(bool diagram, bool hideError)
        {
            return Get("Lamp", diagram, hideError);
        }
        public static Image LDR(bool diagram, bool hideError)
        {
            return Get("LDR", diagram, hideError);
        }
        public static Image LED(bool diagram, bool hideError)
        {
            return Get("LED", diagram, hideError);
        }
        public static Image Loudspeaker(bool diagram, bool hideError)
        {
            return Get("Loudspeaker", diagram, hideError);
        }
        public static Image Microphone(bool diagram, bool hideError)
        {
            return Get("Microphone", diagram, hideError);
        }
        public static Image Switch(bool diagram, bool hideError)
        {
            return Get("Switch", diagram, hideError);
        }
        public static Image Thermistor(bool diagram, bool hideError)
        {
            return Get("Thermistor", diagram, hideError);
        }
        public static Image Transformer(bool diagram, bool hideError)
        {
            return Get("Transformer", diagram, hideError);
        }
        public static Image VariableResistor(bool diagram, bool hideError)
        {
            return Get("VariableResistor", diagram, hideError);
        }
        public static Image Voltmeter(bool diagram, bool hideError)
        {
            return Get("Voltmeter", diagram, hideError);
        }

        private static Image Get(string name, bool diagram, bool hideError)
        {
            return diagram ? GetDiagram(name, hideError) : GetDraw(name, hideError);
        }
        private static Image GetDraw(string name, bool hideError)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "img", "draw", name + ".jpg");
            try
            {
                return Image.FromFile(path);
            } catch
            {
                if (!hideError)
                {
                    MessageBox.Show($"No drawing image found for {name}\nExpected file at '{path}'");
                }
                return NoImageFound();
            }
            
        }
        private static Image GetDiagram(string name, bool hideError)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "img", "diagram", name + ".jpg");
            try
            {
                return Image.FromFile(path);
            } catch
            {
                if (!hideError)
                {
                    MessageBox.Show($"No diagram image found for {name}\nExpected file at '{path}'");
                }
                return NoImageFound();
            }
        }

        private static Image NoImageFound()
        {
            return new Bitmap(300, 300);
        }
    }
}
