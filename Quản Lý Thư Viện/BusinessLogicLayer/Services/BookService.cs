﻿using DataAccessLayer;
using DataAccessLayer.Data;
using DataTransferObject;

namespace BusinessLogicLayer
{
    public class BookService : Service<Book>
    {
        protected override Data<Book> Entity()
        {
            return new BookData();
        }
    }
}
