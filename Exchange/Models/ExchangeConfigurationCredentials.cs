﻿using System;

namespace Exchange
{
    public class ExchangeConfigurationCredentials
    {
        public Guid Id { get; set; }

        public string ExchangeName { get; set; }

        public string ConnectionUrl { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }

        public string Username { get; set; }

        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (ExchangeConfigurationCredentials)obj;
            if (other.ConnectionUrl != ConnectionUrl) return false;
            if (other.Domain != Domain) return false;
            if (other.ExchangeName != ExchangeName) return false;
            if (other.Id != Id) return false;
            if (other.Password != Password) return false;
            if (other.Username != Username) return false;
            return true;
        }

        public override int GetHashCode()
        {
            //GetHashCode generated by Resharper, based on best practises
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (ExchangeName != null ? ExchangeName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConnectionUrl != null ? ConnectionUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Username != null ? Username.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
