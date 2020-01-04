﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using BusinessLogicLayer;
using Core;
using DataTransferObject;

namespace PresentationLayer.Screen
{
    internal enum Column
    {
        Id,
        Name,
        Email,
        Birthday,
    }

    public partial class CustomerScreen : Form
    {
        private readonly ICustomerService customerService = new CustomerService();
        private readonly string txtSearchPlaceholderText = "Nhập tên hoặc địa chỉ email để tìm kiếm...";
        private Customer oldCustomerData;

        public CustomerScreen()
        {
            this.InitializeComponent();
        }

        private void CustomerScreen_Load(object sender, System.EventArgs e)
        {
            this.LoadAll();
        }

        /// <summary>
        /// Lấy tất cả khách hàng đưa vào DataGridView.
        /// </summary>
        private async void LoadAll()
        {
            this.customerBindingSource.DataSource = (await this.customerService.All()).ToList();
        }

        /// <summary>
        /// Kết hợp với hàm <see cref="TxtSearch_Enter"/> để tạo hiệu ứng Placeholder.
        /// </summary>
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            string value = this.txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(value))
            {
                this.LoadAll();
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                ICollection<Customer> customers = this.FindByNameOrEmail(value);
                this.customerBindingSource.DataSource = customers;
            }
        }

        /// <summary>
        /// Kết hợp với hàm <see cref="TxtSearch_KeyDown"/> để tạo hiệu ứng Placeholder.
        /// </summary>
        private void TxtSearch_Enter(object sender, EventArgs e)
        {
            string value = this.txtSearch.Text.Trim();

            if (value == this.txtSearchPlaceholderText)
            {
                this.txtSearch.Text = string.Empty;
            }
        }

        private void TxtSearch_Leave(object sender, EventArgs e)
        {
            string value = this.txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(value))
            {
                this.txtSearch.Text = this.txtSearchPlaceholderText;
            }
        }

        /// <summary>
        /// Tìm kiếm <paramref name="value"/> theo địa chỉ email hoặc tên.
        /// </summary>
        /// <param name="value">Giá trị cần tìm kiếm.</param>
        /// <returns>Các bản ghi có giá trị phù hợp.</returns>
        private ICollection<Customer> FindByNameOrEmail(string value)
        {
            ICollection<Customer> customers = this.customerService.FindByNameOrEmail(value).ToList();

            return customers;
        }

        /// <summary>
        /// Lưu lại dữ liệu để phục hồi lại khi dữ liệu mới không hợp lệ.
        /// </summary>
        private void DataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            this.oldCustomerData = this.GetCustomerAtSelectedRow();
        }

        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            Customer customer = this.GetCustomerAtSelectedRow();

            // Trường hợp không có gì thay đổi
            if (customer == this.oldCustomerData)
            {
                return;
            }

            try
            {
                this.customerService.Update(customer);

                MessageBox.Show("Cập nhật thông tin thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }
            catch (DbUpdateException exception)
            {
                // Trường hợp bị trùng
                if (exception.InnerException.InnerException is SqlException innerException && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    MessageBox.Show($"Dữ liệu bạn nhập đã tồn tại trong cơ sở dữ liệu", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Đã xảy ra lỗi khi cập nhật thông tin", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Phục hồi lại dữ liệu cũ
                this.RejectEdit();
            }
        }

        /// <summary>
        /// Lấy thông tin khách hàng tại hàng đang được chọn trong DataGridView.
        /// </summary>
        /// <returns>Thông tin khách hàng.</returns>
        private Customer GetCustomerAtSelectedRow()
        {
            return (Customer)((Customer)this.dataGridView.SelectedRows[0].DataBoundItem).Clone();
        }

        /// <summary>
        /// Gán lại dữ liệu cũ vào cột đang được chọn.
        /// </summary>
        private void RejectEdit()
        {
            this.dataGridView.SelectedRows[0].Cells[0].Value = this.oldCustomerData.Id;
            this.dataGridView.SelectedRows[0].Cells[1].Value = this.oldCustomerData.Name;
            this.dataGridView.SelectedRows[0].Cells[2].Value = this.oldCustomerData.Email;
            this.dataGridView.SelectedRows[0].Cells[3].Value = this.oldCustomerData.Birthday;
        }

        /// <summary>
        /// Thực hiện kiểm tra tính đúng đắn của dữ liệu khi người dùng nhập.
        /// </summary>
        private void DataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            string value = e.FormattedValue.ToString();

            switch (e.ColumnIndex)
            {
                case (int)Column.Name:
                    e.Cancel = this.ValidateName(value);
                    break;
                case (int)Column.Email:
                    e.Cancel = this.ValidateEmail(value);
                    break;
                case (int)Column.Birthday:
                    e.Cancel = this.ValidateBirthday(value);
                    break;
            }
        }

        /// <summary>
        /// Kiểm tra <paramref name="name"/> có phải là tên hợp lệ.
        /// </summary>
        /// <param name="name">Họ và tên.</param>
        /// <returns><see langword="true"/> nếu không hợp lệ, <see langword="false"/> nếu hợp lệ.</returns>
        /// <value>hello</value>
        private bool ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Họ tên không được để trống", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra <paramref name="email"/> có phải là một địa chỉ email hợp lệ.
        /// </summary>
        /// <param name="email">Địa chỉ email.</param>
        /// <returns><see langword="true"/> nếu không hợp lệ, <see langword="false"/> nếu hợp lệ.</returns>
        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Địa chỉ email không được để trống", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }

            if (!Validation.IsEmail(email))
            {
                MessageBox.Show("Địa chỉ email không hợp lệ", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra <paramref name="birthday"/> có phải là một ngày sinh hợp lệ.
        /// </summary>
        /// <param name="birthday">Ngày sinh.</param>
        /// <returns><see langword="true"/> nếu không hợp lệ, <see langword="false"/> nếu hợp lệ.</returns>
        private bool ValidateBirthday(string birthday)
        {
            if (string.IsNullOrEmpty(birthday))
            {
                MessageBox.Show("Ngày tháng năm sinh không được để trống", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }

            if (!DateTime.TryParse(birthday, out var _))
            {
                MessageBox.Show("Ngày tháng năm sinh không hợp lệ", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }

            return false;
        }
    }
}
