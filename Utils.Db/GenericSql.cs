using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Utils.Db
{
    public class GenericSql : IDisposable
    {
        private SqlConnection _Conexion;
        private SqlCommand _Comando = null;
        private SqlTransaction _Transaccion = null;

        public GenericSql()
        {
            _Conexion = new SqlConnection(ConfigurationManager.ConnectionStrings["DATABASE"].ConnectionString);
            _Conexion.Open();
        }

        public void Desconectar()
        {
            if (_Conexion is not null && _Conexion.State is ConnectionState.Open)
            {
                _Conexion.Close();
            }
        }

        public void CrearTransaccion()
        {
            _Transaccion = _Conexion.BeginTransaction();
        }

        public void AsentarTransaccion()
        {
            _Transaccion?.Commit();
        }

        public void CancelarTransaccion()
        {
            _Transaccion?.Rollback();
            _Transaccion = null;
        }

        protected void CrearComando(string sql)
        {
            _Comando = new SqlCommand(sql, _Conexion, _Transaccion);
        }

        protected void CrearComandoSP(string sql)
        {
            _Comando = new SqlCommand(sql, _Conexion)
            {
                CommandType = CommandType.StoredProcedure
            };
        }

        protected void AgregarParametro(string nombre, object valor)
        {
            _Comando.Parameters.AddWithValue(nombre, valor);
        }

        protected void AgregarParametroSP(string nombre, DbType tipo, object valor, ParameterDirection direccion)
        {
            SqlParameter parametro = _Comando.CreateParameter();
            parametro.ParameterName = nombre;
            parametro.DbType = tipo;
            switch (parametro.DbType)
            {
                case DbType.String: parametro.Size = -1; break;
                case DbType.Binary: parametro.Size = int.MaxValue; break;
                default: break;
            }
            parametro.Value = valor;
            parametro.Direction = direccion;
            _Comando.Parameters.Add(parametro);
        }

        protected object ObtenerParametro(string nombre)
        {
            return _Comando.Parameters[nombre].Value == DBNull.Value ? null : _Comando.Parameters[nombre].Value;
        }

        protected object NullToDbNull(object value)
        {
            return value is null || string.IsNullOrEmpty(value.ToString()) ? DBNull.Value : value;
        }

        protected static DateTime? DbNullToDateTime(object value)
        {
            return value == DBNull.Value ? null : (DateTime)value;
        }

        protected static string DbNullToString(object value)
        {
            return value == DBNull.Value ? null : value.ToString();
        }

        protected SqlDataReader EjecutarConsulta()
        {
            return _Comando.ExecuteReader();
        }

        protected int EjecutarComando()
        {
            return _Comando.ExecuteNonQuery();
        }

        protected object EjecutarEscalar()
        {
            return _Comando.ExecuteScalar();
        }

        protected void CloseReader(SqlDataReader reader)
        {
            if (reader is not null)
            {
                reader.Close();
                reader.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _Conexion?.Dispose();
            _Comando?.Dispose();
            _Transaccion?.Dispose();

            _Conexion = null;
            _Comando = null;
            _Transaccion = null;
        }
    }
}
