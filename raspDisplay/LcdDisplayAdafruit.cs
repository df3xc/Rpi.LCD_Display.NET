using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RPi.I2C.Net;
using System.Threading;



namespace LcdDisplay
{
    public class Display

    {
       // static I2CBus i2cbus = new I2CBus("/dev/i2c-1");

        static I2CBus i2cbus = I2CBus.Open("/dev/i2c-1");


        /// <summary>
        /// shift and reverse byte to LCD data bit 4..7
        /// </summary>
        static byte[] flip = { 0x00,0x10,0x08,0x18,
                               0x04,0x14,0x0c,0x1c,
                               0x02,0x12,0x0a,0x1a,
                               0x06,0x16,0x0e,0x1e };

       static Boolean inuse = false;


       public static void White()
        {
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olatb, 0x0);
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olata, 0x0);
        }

       public static void Red()
        {
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olatb, 0x1);
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olata, 0x80);
        }

       public static void Green()
        {
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olatb, 0x1);
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olata, 0x40);
        }

       public static void Blue()
        {
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olatb, 0x0);
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olata, 0x40);
        }

       public static void Magenta()
        {
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olatb, 0x0);
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olata, 0x80);
        }

       public static void Yellow()
        {
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olatb, 0x1);
            i2cbus.WriteCommand(mpc_bank0.address, mpc_bank0.olata, 0x0);
        }

       public static void Clear()
        {
            lcd_writeCommand(0x1);
            lcd_writeCommand(0x2);
        }

       public static void Line2()
       {
           lcd_writeCommand(0x68);
           lcd_writeCommand(0xA8);

       }

        /// <summary>
        /// write to MPC register
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="val"></param>

        static void mpc_write(byte reg, byte val, int sleep)
        {
            i2cbus.WriteCommand(mpc_bank0.address, reg, val);
            //Console.WriteLine("IC2WR " + reg.ToString("X2") + ":" +  val.ToString("X2"));
            Thread.Sleep(sleep);
        }

        /// <summary>
        /// write command to LCD (R/S=0)
        /// </summary>
        /// <param name="cmd"></param>

        static void lcd_writeCommand(byte cmd)
        {
            byte data;

            //Console.WriteLine("WriteCommand:" + cmd.ToString("X2"));

            data = (byte)(cmd >> 4);
            data = (byte)(data & 0x0F);
            data = flip[data];

            mpc_write(mpc_bank0.olatb, (byte)(0x00 + data),10);
            mpc_write(mpc_bank0.olatb, (byte)(0x20 + data), 10);
            mpc_write(mpc_bank0.olatb, (byte)(0x00 + data), 10);


            data = (byte)(0x0F & cmd);
            data = flip[data];

            mpc_write(mpc_bank0.olatb, (byte)(0x00 + data), 10);
            mpc_write(mpc_bank0.olatb, (byte)(0x20 + data), 10);
            mpc_write(mpc_bank0.olatb, (byte)(0x00 + data), 10);

        }


        /// <summary>
        /// write data to LCD (R/S=1)
        /// </summary>
        /// <param name="cmd"></param>

        static void lcd_writeData(byte cmd)
        {

            byte data;

            //Console.WriteLine("WriteData:" + cmd.ToString("X2"));

            data = (byte)(cmd >> 4);
            data = (byte)(data & 0x0F);
            data = flip[data];

            mpc_write(mpc_bank0.olatb, (byte)(0x80 + data), 2);
            mpc_write(mpc_bank0.olatb, (byte)(0xA0 + data), 2);
            mpc_write(mpc_bank0.olatb, (byte)(0x80 + data), 2);

            data = (byte)(0x0F & cmd);
            data = flip[data];

            mpc_write(mpc_bank0.olatb, (byte)(0x80 + data), 2);
            mpc_write(mpc_bank0.olatb, (byte)(0xA0 + data), 2);
            mpc_write(mpc_bank0.olatb, (byte)(0x80 + data), 2);
        }

        public static void lcd_init()
        {

            mpc_init();

            lcd_writeCommand(0x33);
            lcd_writeCommand(0x32);
            lcd_writeCommand(0x28);

            lcd_writeCommand(0x1);
            lcd_writeCommand(0x2);
            lcd_writeCommand(0x6);

            lcd_writeCommand(lcdcmd.LCD_DISPLAYCONTROL +  lcdcmd.DISPLAYON);

        }


        public static void Write(string text)
        {
            if (inuse == true)
            {
                return;
            }

            inuse = true;
            foreach (char c in text)
            {
                lcd_writeData((byte)c);
            }
            inuse = false;

        }

        public static byte readkey()
        {
            byte key;
            byte[] data;
            data = i2cbus.ReadDeviceRegister(mpc_bank0.address, mpc_bank0.gpioa);
            data[0] = (byte)~data[0];
            data[0] = (byte)(0x1F & data[0]);
            key = data[0];

            do
            {
                Thread.Sleep(50);
                data = i2cbus.ReadDeviceRegister(mpc_bank0.address, mpc_bank0.gpioa);
                data[0] = (byte)~data[0];
                data[0] = (byte)(0x1F & data[0]);
            } while (data[0] != 0);

            return (key);

        }

        public static void mpc_init()
        {
            mpc_write(mpc_bank0.iodira, 0x3F, 10);
            mpc_write(mpc_bank0.gppua, 0x3F, 10);
            mpc_write(mpc_bank0.iodirb, 0x00, 10);
 
        }

        static void Main(string[] args)
        {

            mpc_init();

            White();
            Thread.Sleep(200);

            Blue();
            Thread.Sleep(200);

            Green();
            Thread.Sleep(200);

            Red();
            Thread.Sleep(200);

            lcd_init();

            Write(" Guten Tag ");

            byte key;
            for (int k = 0; k < 500; k++)
            {

               key = readkey();

               if ((key != 0x0))
               {
                   Console.WriteLine("Key:" + key.ToString("X2"));
                   Clear();
                   Write("Key:" + key.ToString("X2"));
                }
               Thread.Sleep(200);
            }

        }
    }


    /// <summary>
    /// MPC register addresses in BANK0
    /// </summary>

    public class mpc_bank0
    {

        public const int iodira = 0x00;
        public const int iodirb = 0x01;
        public const int gpioa  = 0x12;
        public const int gpiob = 0x13;
        public const int olata = 0x14;
        public const int olatb = 0x15;
        public const int gppua = 0x0C;

        /// <summary>
        /// MPC i2c device address
        /// </summary>
        public const int address = 0x20; 

    }


    public class lcdcmd
    {
        public const byte LCD_CLEARDISPLAY        = 0x01;
        public const byte LCD_RETURNHOME          = 0x02;
        public const byte LCD_ENTRYMODESET        = 0x04;
        public const byte LCD_DISPLAYCONTROL      = 0x08;
        public const byte   LCD_CURSORSHIFT         = 0x10;
        public const byte  LCD_FUNCTIONSET         = 0x20;
        public const byte LCD_SETCGRAMADDR = 0x40;

        public const byte DISPLAYON = 0x04;
        public const byte DISPLAYOFF          = 0x00;
        public const byte CURSORON            = 0x02;
        public const byte CURSOROFF           = 0x00;
        public const byte BLINKON             = 0x01;
        public const byte BLINKOFF            = 0x00;

        public const byte ENTRYRIGHT          = 0x00;
        public const byte ENTRYLEFT           = 0x02;
        public const byte ENTRYSHIFTINCREMENT = 0x01;
        public const byte ENTRYSHIFTDECREMENT = 0x00;
        
        public const byte DISPLAYMOVE = 0x08;
        public const byte CURSORMOVE  = 0x00;
        public const byte MOVERIGHT   = 0x04;
        public const byte MOVELEFT = 0x00;
    }



}
