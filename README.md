Rpi.LCD_Display.NET
===================

Control RaspBerry LCD Display Plate using C#/.NET

This library 

- controls a I2C bus MPC23017 port expander.
- controls a LCD Display attached to the MPC23017 port expander
- read keypad attached to the MPC23017 port expander


The library is used and tested with Adafruit 16x2 Character LCD + Keypad for Raspberry Pi

It provides:
- Set the LCD background color (if display supports RGB)
- Write text to the display
- Red keys from keypad

uses df3xc/RPi.I2C.Net forked from mshmelev/RPi.I2C.Net 
