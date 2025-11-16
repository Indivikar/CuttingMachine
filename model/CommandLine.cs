using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchneidMaschine.model
{
    public class CommandLine
    {
        private DataModel dataModel;

        private COMMAND_Schneidmaschine command;
        private int steps;
        private DIRECTION direction;

        public CommandLine(DataModel dataModel)
        {
            this.dataModel = dataModel;
        }

        public string getCommandLine() {

            string comm = command.ToString();
            string direc = direction.ToString();

            return comm + "_" + steps + "_" + direc;
        }

        public void DirectionAsBool(bool wert) {
            if (wert) {
                direction = DIRECTION.forward;
            } else {
                direction = DIRECTION.backward;
            }
        }




        public void setCommandLine(COMMAND_Schneidmaschine command, int stepsAsMM, bool wert)
        {
            if (wert)
            {
                setCommandLine(command, dataModel.mmToSteps(stepsAsMM), DIRECTION.backward);              
            }
            else
            {
                setCommandLine(command, dataModel.mmToSteps(stepsAsMM), DIRECTION.forward);
            }
        }

        public void setCommandLine(COMMAND_Schneidmaschine command, int steps, DIRECTION direction)
        {
            this.command = command;
            this.steps = steps;
            this.direction = direction;

            Console.WriteLine("setCommandLine -> " + command + "_" + steps + "-" + direction);

            if (COMMAND_Schneidmaschine.schneidenStart.Equals(command)) {
                Console.WriteLine("set dataModel.IsCutFinished = false");
                dataModel.IsCutFinished = false;
            }

            if (COMMAND_Schneidmaschine.stepperStart.Equals(command))
            {
                Console.WriteLine("set dataModel.IsStepperFinished = false");
                dataModel.IsStepperFinished = false;
            }
        }

        public COMMAND_Schneidmaschine Command { get => command; set => command = value; }
        public int Steps { get => steps; set => steps = value; }
        public DIRECTION Direction { get => direction; set => direction = value; }
    }
}
