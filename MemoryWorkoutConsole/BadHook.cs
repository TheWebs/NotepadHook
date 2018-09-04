using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryWorkoutConsole
{
    class BadHook
    {
        //importar DLL nativo para trabalhar a memoria
        [DllImport("kernel32.dll")]
        public static extern int OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, long lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesWritten);

        //criar as flags para ler/escrever/apagar etc ..
        static uint DELETE = 0x00010000;
        static uint READ_CONTROL = 0x00020000;
        static uint WRITE_DAC = 0x00040000;
        static uint WRITE_OWNER = 0x00080000;
        static uint SYNCHRONIZE = 0x00100000;
        static uint END = 0xFFF; //if you have Windows XP or Windows Server 2003 you must change this to 0xFFFF
        static uint PROCESS_ALL_ACCESS = (DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER | SYNCHRONIZE | END);
        Thread thread;

        public BadHook()
        {
            doWork();
        }
        //notepad.exe + 24548
        //MSCTF.dll+11E6E0
        public void doWork()
        {
            Process[] p = Process.GetProcessesByName("notepad");
            int processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, p[0].Id);
            IntPtr baseAdd = p[0].MainModule.BaseAddress;
            Console.WriteLine("Base address: " + baseAdd.ToString("X"));
            IntPtr firstAdd = IntPtr.Add(baseAdd, 0x00024548);
            Console.WriteLine("offseted BaseAddress: " + firstAdd.ToString("X"));
            byte[] firstAddVal = ReadMemory(firstAdd.ToInt64(), sizeof(UInt64), processHandle);
            Console.WriteLine("First addValue: " + BitConverter.ToUInt64(firstAddVal, 0).ToString("X"));
            IntPtr secondAdd = (IntPtr)BitConverter.ToUInt64(firstAddVal, 0);
            IntPtr.Add(secondAdd, 0);
            Console.WriteLine("Second address: " + secondAdd.ToString("X"));
            byte[] secondAddVal = ReadMemory(secondAdd.ToInt64(), sizeof(UInt64), processHandle);
            Console.WriteLine("Second addValue: " + BitConverter.ToUInt64(secondAddVal, 0).ToString("X"));
            IntPtr thirdAdd = (IntPtr)BitConverter.ToUInt64(secondAddVal, 0);
            Console.WriteLine("Third address: " + thirdAdd.ToString("X"));
            IntPtr.Add(thirdAdd, 0);
            byte[] thirdAddVal = ReadMemory(thirdAdd.ToInt64(), 200, processHandle);
            Console.WriteLine("Third addValue: " + Encoding.Unicode.GetString(thirdAddVal));
            //Console.WriteLine((Encoding.Unicode.GetString(ReadMemory(0x1F229DD8E60, 200, processHandle))));
            Console.ReadKey();
            while(true)
            {
                byte[] texto = ReadMemory(thirdAdd.ToInt64(), 200, processHandle);
                Console.WriteLine(Encoding.Unicode.GetString(texto));
                System.Threading.Thread.Sleep(500);
                Console.Clear();
            }
        }


        //Para simplificar as coisas e "converter" para metodos menos chatos

        public byte[] ReadMemory(long adress, int processSize, int processHandle)
        {
            byte[] buffer = new byte[processSize];
            ReadProcessMemory(processHandle, adress, buffer, processSize, 0);
            return buffer;
        }

        public void WriteMemory(int adress, byte[] processBytes, int processHandle)
        {
            WriteProcessMemory(processHandle, adress, processBytes, processBytes.Length, 0);
        }
    }
}
