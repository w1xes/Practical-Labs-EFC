// Previews the cost of each line and of the whole order while quantities are typed.
(() => {
    const rows = document.querySelectorAll('tr[data-price]');
    const total = document.getElementById('total-cost');

    const update = () => {
        let sum = 0;
        rows.forEach(row => {
            const quantity = Number(row.querySelector('.quantity').value);
            const cost = quantity > 0 ? quantity * Number(row.dataset.price) : 0;
            row.querySelector('.line-cost').textContent = cost.toFixed(2);
            sum += cost;
        });
        total.textContent = sum.toFixed(2);
    };

    rows.forEach(row => row.querySelector('.quantity').addEventListener('input', update));
    update();
})();
