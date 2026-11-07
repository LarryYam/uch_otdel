// Каркас «Учебного отдела»: прокрутка ленты команд в шапке стрелками ◀ ▶
window.uchShell = {
    scrollBy: function (element, dx) {
        if (element) {
            element.scrollBy({ left: dx, behavior: 'smooth' });
        }
    }
};
