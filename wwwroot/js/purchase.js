function roundNumber(number, decimals) {
	var newString;
	decimals = Number(decimals);
	if (decimals < 1) {
		newString = (Math.round(number)).toString();
	} else {
		var numString = number.toString();
		if (numString.lastIndexOf(".") == -1) {
			numString += ".";
		}
		var cutoff = numString.lastIndexOf(".") + decimals;
		var d1 = Number(numString.substring(cutoff, cutoff + 1));
		var d2 = Number(numString.substring(cutoff + 1, cutoff + 2));
		if (d2 >= 5) {
			if (d1 == 9 && cutoff > 0) {
				while (cutoff > 0 && (d1 == 9 || isNaN(d1))) {
					if (d1 != ".") {
						cutoff -= 1;
						d1 = Number(numString.substring(cutoff, cutoff + 1));
					} else {
						cutoff -= 1;
					}
				}
			}
			d1 += 1;
		}
		if (d1 == 10) {
			numString = numString.substring(0, numString.lastIndexOf("."));
			var roundedNum = Number(numString) + 1;
			newString = roundedNum.toString() + '.';
		} else {
			newString = numString.substring(0, cutoff) + d1.toString();
		}
	}
	if (newString.lastIndexOf(".") == -1) {
		newString += ".";
	}
	var decs = (newString.substring(newString.lastIndexOf(".") + 1)).length;
	for (var i = 0; i < decimals - decs; i++) newString += "0";
	return newString;
}

function updateTotal() {


	var total = 0, discount = 0;

	$('.item-gross').each(function (i) {
		price = $(this).val();
		if (!isNaN(price)) { total += Number(price); }
	});

	var taxtotal = 0;
	$('.item-taxprice').each(function (i) {
		taxprice = $(this).val();
		if (!isNaN(taxprice)) { taxtotal += Number(taxprice); }
	});

	$('.item-discountvalue').each(function (i) {
		var value = $(this).val();
		if (!isNaN(value)) { discount += Number(value); }
	});

	total = roundNumber(total, 2);
	taxprice = roundNumber(taxtotal, 2);
	discount = roundNumber(discount, 2);

	$('.total-price').val(total);
	$('.total-tax').val(taxprice);
	$('.total-discount').val(discount);

	var amount = total - discount + parseFloat(taxprice);

	amount = Number(roundNumber(amount, 2));

	$('.total-amount').val(amount);
}

function updatePrice() {
	$('.item-row').each(function () {
		var row = $(this),
			price = row.find('.item-purchaseprice').val() * row.find('.item-qty').val();

		price = Number(roundNumber(price, 2));
		row.find('.item-gross').val(price);

		discounttype = row.find('.item-discounttype').val(),
			discount = Number(row.find('.item-discount').val());

		discount = roundNumber(discount, 2);
		if (discounttype === "2") {
			price = price - discount;
			row.find('.item-discountvalue').val(discount);
		} else {
			discount = price * discount * 0.01;
			discount = Number(roundNumber(discount, 2));
			row.find('.item-discountvalue').val(discount);
			price = price - discount;
		}

		var tax = Number(row.find('.item-tax').val());
		tax_price = price * tax * 0.01;
		tax_price = Number(roundNumber(tax_price, 2));
		row.find('.item-taxprice').val(tax_price);

		price += tax_price;
		price = Number(roundNumber(price, 2));

		isNaN(price) ? row.find('.item-price').val("N/A") : row.find('.item-price').val(price);
	});
	updateTotal();
}

function bind() {
	$(".item-purchaseprice").on('input', updatePrice);
	$(".item-qty").on('input', updatePrice);
	$(".item-discounttype").on('input', updatePrice);
	$(".item-discount").on('input', updatePrice);
	$(".item-tax").on('input', updatePrice);
	$("body").on('change', '.discount-type', updatePrice);
}

function initDatepicker() {
	$('.exp-date').datetimepicker({
		viewMode: 'years',
		format: "MM-YYYY",
		widgetPositioning: {
			horizontal: 'auto',
			vertical: 'auto'
		},
		collapse: true,
		icons: {
			time: "far fa-clock",
			date: "far fa-calendar-alt",
			up: "fas fa-angle-up",
			down: "fas fa-angle-down",
			previous: 'fas fa-angle-left',
			next: 'fas fa-angle-right',

		}
	});
}

function initAutocomplete() {
	$(".item-name").autocomplete({
		minLength: 0,
		source: ('/getmedicine'),
		focus: function (event, ui) {
			$(this).parents('tr').find('.item-name').val(ui.item.label);
			return false;
		},
		select: function (event, ui) {
			$(this).parents('tr').find('.item-name').val(ui.item.label);
			$(this).parents('tr').find('.item-id').val(ui.item.id);
			$(this).removeClass('ui-state-error');
			return false;
		}
	});
}

function item_html(count) {
	var item_html = '<tr class="item-row">' +
		'<td>' +
		'<textarea class="item-name" name="Items[' + count + '].Name" required></textarea>' +
		'<input type="hidden" class="item-id" name="Items[' + count + '].MedicineId">' +
		'<input type="hidden" name="Items[' + count + '].Id">' +
		'</td>' +
		'<td>' +
		'<textarea class="item-batch" name="Items[' + count + '].Batch" required></textarea>' +
		'</td>' +
		'<td>' +
		'<input type="text" class="item-expiry exp-date p-2 datetimepicker-input bg-white" name="Items[' + count + '].Expiry" required>' +
		'</td>' +
		'<td>' +
		'<textarea class="item-pqty" name="Items[' + count + '].Pqty"></textarea>' +
		'</td>' +
		'<td>' +
		'<textarea class="item-qty" name="Items[' + count + '].Qty" required></textarea>' +
		'</td>' +
		'<td>' +
		'<textarea class="item-purchaseprice" name="Items[' + count + '].Purchaseprice" required></textarea>' +
		'</td>' +
		'<td>' +
		'<textarea class="item-saleprice" name="Items[' + count + '].Saleprice" required></textarea>' +
		'</td>' +
		'<td>' +
		'<input class="item-gross" readonly name="Items[' + count + '].Gross" value="0" />' +
		'</td>' +
		'<td class="">' +
		'<div class="row no-gutters">' +
		'<div class="col">' +
		'<select name="Items[' + count + '].Discounttype" class="custom-select border-light item-discounttype">' +
		'<option value="1">%</option>' +
		'<option value="2">Flat</option>' +
		'</select>' +
		'</div>' +
		'<div class="col">' +
		'<textarea type="text" name="Items[' + count + '].Discount" class="item-discount">0</textarea>' +
		'<input type="text" readonly name="Items[' + count + '].Discountvalue" class="item-discountvalue" value="0">' +
		'</div>' +
		'</div>' +
		'</td>' +
		'<td class="invoice-tax">' +
		'<input type="text" name="Items[' + count + '].Tax" class="item-tax" value="0">' +
		'<input type="text" readonly name="Items[' + count + '].Taxprice" class="item-taxprice" value="0">' +
		'</td>' +
		'<td>' +
		'<textarea class="bg-white item-price" name="Items[' + count + '].Price" required readonly></textarea>' +
		'</td>' +
		'<td>' +
		'<a class="badge badge-danger badge-sm badge-pill delete m-1">Delete</a>' +
		'</td>' +
		'</tr>';

	if (count === 0) {
		$(".purchase-items tbody").prepend(item_html);
	} else {
		$(".purchase-items .item-row:last").after(item_html);
	}
}

$(document).ready(function () {
	"use strict";

	$('.purchase-items').on('click', '.add-items', function () {
		if ($(".item-row").length === 0) {
			item_html(0);
		} else {
			var count = $('.purchase-items table tr.item-row:last .item-name').attr('name').split('[')[1];
			count = parseInt(count.split(']')[0]) + 1;
			item_html(count);
		}
		initAutocomplete();
		initDatepicker();
		bind();
	});

	$('.purchase-items').on('click', '.delete', function () {
		var ele = $(this);
		ele.parents('.item-row').remove();
		bind();
		return false;
	});

	$(".item-name").on('input', function () {
		const ethis = $(this);
		ethis.next().val('');
		ethis.addClass('ui-state-error');
	});

	$('.purchase_form').submit(function (e) {

		$('.item-id').each(function () {
			const itemEl = $(this);
			const item_id = itemEl.val();

			if (item_id == "") {

				e.preventDefault();

				itemEl.prev().addClass('ui-state-error');
				itemEl.focus();

			} else {
				itemEl.prev().removeClass('ui-state-error');
			}

		});

	});

	initAutocomplete();
	initDatepicker();
	bind();

});