using System.Collections.Generic;

namespace GymTracker.Data
{
    /// <summary>
    /// Clase base abstracta para todos los DAOs.
    /// Define el contrato CRUD que cada DAO concreto debe implementar.
    /// </summary>
    public abstract class BaseDAO<T>
    {
        protected string ConnectionString => DatabaseHelper.ConnectionString;

        public abstract List<T> GetAll();
        public abstract T GetById(int id);
        public abstract bool Insert(T entity);
        public abstract bool Update(T entity);
        public abstract bool Delete(int id);
    }
}
