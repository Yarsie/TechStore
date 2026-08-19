using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechStore.Domain.Entities
{
        public class Cart
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
