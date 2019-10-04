using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Vue.Core.Common;
using Vue.Core.Data;
using Vue.Core.Data.Entities;
using Vue.Core.Model;
using Vue.Core.Service.Interface;

namespace Vue.Core.Service
{
    public class UsersService : IUsersService<Users>
    {
        private ApplicationDbContext _db;
        public UsersService(ApplicationDbContext db) => _db = db;

        public PagingModel<Users> GetAll(Fitlers.UsersFilter filter)
        {
            var query = filter.Query(_db.Users.Where(x=>!x.IsDeleted));
            return query;
        }

        public Users GetById(int id)
        {
            return _db.Users.Find(id);
        }
        
        public Users GetByGId(Guid Gid)
        {
            return _db.Users.Include("UsersRoles.Roles").FirstOrDefault(x => x.Gid==Gid);
        }

        public Users Create(Users users)
        {
            // validation
            if (string.IsNullOrWhiteSpace(users.Password))
                throw new AppException("Password is required");

            if (_db.Users.Any(x => x.LoginName == users.LoginName))
                throw new AppException("Username \"" + users.LoginName + "\" is already taken");

            byte[] passwordHash, passwordSalt;
            Security.CreatePasswordHash(users.Password, out passwordHash, out passwordSalt);

            users.PasswordHash = passwordHash;
            users.PasswordSalt = passwordSalt;
            users.Id = default;
            _db.Users.Add(users);
            _db.SaveChanges();

            return users;
        }

        public void Update(Users t)
        {
            var u = _db.Users.FirstOrDefault(x => x.Gid == t.Gid);
            if (u != null)
            {
                u.FirstName = t.FirstName;
                u.LastName = t.LastName;
                u.Email = t.Email;
                u.Gender = t.Gender;
                u.DateOfBirth = t.DateOfBirth;
                _db.SaveChanges();
            }

            
        }

        public void Delete(int it)
        {
            throw new System.NotImplementedException();
        }
        
        public Users Authenticate(string loginname, string password)
        {
            if (string.IsNullOrEmpty(loginname) || string.IsNullOrEmpty(password))
                return null;

            var user = _db.Users.SingleOrDefault(x => x.LoginName == loginname);
            
            if (user == null)
                return null;

            // check if password is correct
            if (!Security.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }
    }
}