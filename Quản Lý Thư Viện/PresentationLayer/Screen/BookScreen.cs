﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BusinessLogicLayer;
using DataTransferObject;
using PresentationLayer.Screen.Childs;

namespace PresentationLayer.Screen
{
    public partial class BookScreen : Form
    {
        private readonly IBookService bookService = new BookService();

        public BookScreen()
        {
            this.InitializeComponent();
        }

        private void BookScreen_Load(object sender, EventArgs e)
        {
            this.LoadAll();
        }

        private void LoadAll()
        {
            this.BindGrid(this.bookService.All());
        }

        /// <summary>
        /// Kết hợp với hàm <see cref="TxtSearch_Leave"/> để tạo hiệu ứng Placeholder.
        /// </summary>
        private void TxtSearch_Enter(object sender, EventArgs e)
        {
            Control txtSearch = (Control)sender;
            string text = txtSearch.Text.Trim();

            if (text == "Nhập tên sách cần tìm...")
            {
                ((Control)sender).Text = string.Empty;
            }
        }

        /// <summary>
        /// Kết hợp với hàm <see cref="TxtSearch_Enter"/> để tạo hiệu ứng Placeholder.
        /// </summary>
        private void TxtSearch_Leave(object sender, EventArgs e)
        {
            Control txtSearch = (Control)sender;
            string text = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                txtSearch.Text = "Nhập tên sách cần tìm...";
            }
        }

        /// <summary>
        /// Thực hiện tìm kiếm sách theo tên, tác giả, nhà phát hành, thể loại.
        /// </summary>
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            string value = (sender as Control).Text;

            // Nếu xóa hết thì khỏi cần bấm Enter
            if (string.IsNullOrEmpty(value))
            {
                this.LoadAll();
                return;
            }

            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            // Tìm kiếm
            var entities = this.bookService.SearchByNameOrAuthorOrPublisherOrCategory(value).ToList();

            if (entities.Count == 0)
            {
                MessageBox.Show("Không tìm thấy thông tin bạn cần", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                this.BindGrid(entities);
            }
        }

        /// <summary>
        /// Đổ dữ liệu vào DataGridView.
        /// </summary>
        /// <param name="books">Danh sách sách.</param>
        private void BindGrid(IEnumerable<Book> books)
        {
            this.bindingSource.DataSource = books.ToList();
        }

        /// <summary>
        /// Hiển thị menu khi chuột phải vào dòng được chọn trong DataGridView.
        /// </summary>
        private void DataGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            this.dataGridView.ClearSelection();

            if (e.RowIndex > -1)
            {
                this.dataGridView.Rows[e.RowIndex].Selected = true;
                e.ContextMenuStrip = this.contextMenuStrip;
            }
        }

        /// <summary>
        /// Hiển thị Form xem thông tin của sách.
        /// </summary>
        private void SeeMoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Book book = (Book)this.dataGridView.SelectedRows[0].DataBoundItem;
            new BookInfoForm(book.Id).ShowDialog();
            this.LoadAll();
        }

        /// <summary>
        /// Hiển thị Form thêm sách.
        /// </summary>
        private void BtnAddBook_Click(object sender, EventArgs e)
        {
            new InsertBookForm().ShowDialog();
            this.LoadAll();
        }

        /// <summary>
        /// Xóa sách.
        /// </summary>
        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Book book = (Book)this.dataGridView.SelectedRows[0].DataBoundItem;

            this.bookService.Delete(book);

            this.LoadAll();
        }
    }
}
