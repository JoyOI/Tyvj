﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("motto")]
        public string Motto { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("gravatar")]
        public string Gravatar { get; set; }

        [Column("school")]
        public string School { get; set; }

        [Column("sex")]
        public int SexAsInt { get; set; }

        [NotMapped]
        public Sex Sex
        {
            get { return (Sex)SexAsInt; }
            set { SexAsInt = (int)value; }
        }

        [Column("last_login_time")]
        public DateTime LastLoginTime { get; set; }

        [Column("register_time")]
        public DateTime RegisterTime { get; set; }

        [Column("qq")]
        public string QQ { get; set; }

        [Column("role")]
        public int RoleAsInt { get; set; }

        [NotMapped]
        public UserRole Role
        {
            get { return (UserRole)RoleAsInt; }
            set { RoleAsInt = (int)value; }
        }

        [Column("common_language")]
        public int CommonLanguageAsInt { get; set; }

        [NotMapped]
        public Language CommonLanguage
        {
            get { return (Language)CommonLanguageAsInt; }
            set { CommonLanguageAsInt = (int)value; }
        }

        public virtual ICollection<Rating> Ratings { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as User;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum UserRole { Temporary, Member, VIP, Master, Root };
    public enum Sex{Male, Female};
}