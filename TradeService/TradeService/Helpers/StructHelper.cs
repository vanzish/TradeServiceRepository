using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TradeService
{
    public class StructHelper
    {
        private string _file;
        private FileStream _fs;

        public StructHelper(string file)
        {
            _file = file;
        }

        private void LoadFileStream(FileMode FileMode, FileAccess FileAccess, FileShare FileShare)
        {
            if(_fs == null)
            {
                try
                {
                    _fs = new FileStream(_file, FileMode, FileAccess, FileShare);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        public bool EOF
        {
            get
            {
                if(_fs != null)
                {
                    if(_fs.Position >= _fs.Length)
                        Close();
                }

                return _fs == null;
            }
        }

        private byte[] StructToByteArray(object @struct)
        {
            try
            {
                byte[] buffer = new byte[Marshal.SizeOf(@struct)]; 

                GCHandle h = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                Marshal.StructureToPtr(@struct, h.AddrOfPinnedObject(), false);
                h.Free(); 

                return buffer; 
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        public bool WriteStructure(object @struct)
        {
            try
            {
                byte[] buf = StructToByteArray(@struct);

                BinaryWriter bw = new BinaryWriter(_fs);

                bw.Write(buf);

                bw.Close();
                bw = null;

                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public bool WriteStructureCollection(List<object> structCollection)
        {
            try
            {
                BinaryWriter bw = new BinaryWriter(_fs);
                foreach(var @struct in structCollection)
                {
                    byte[] buf = StructToByteArray(@struct);
                    bw.Write(buf);
                }

                bw.Close();
                bw = null;
                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public object GetNextStructureValue(Type type)
        {
            if(EOF) return null;
            return GetStructureValue(type);
        }

        public List<object> GetAllStrucureValues(Type type)
        {
            var structureList = new List<object>();
            while(!EOF)
            {
                structureList.Add(GetStructureValue(type));
            }

            return structureList;
        }

        private object GetStructureValue(Type type)
        {

            byte[] buffer = new byte[Marshal.SizeOf(type)];

            object oReturn;

            try
            {
                _fs.Read(buffer, 0, buffer.Length);

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                oReturn = (object)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
                handle.Free();

                if(_fs.Position >= _fs.Length)
                    Close();

                return oReturn;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void Close() 
        {
            if(_fs != null)
            {
                _fs.Close();
                _fs = null;
            }
            GC.Collect();
        }

        public void Open(FileMode FileMode, FileAccess FileAccess, FileShare FileShare)
        {
            LoadFileStream(FileMode, FileAccess, FileShare);
        }
    }

}
