// Custom JavaScript for Warathatul Ambiya

document.addEventListener('DOMContentLoaded', () => {
    // Fade in elements on page load
    const fadeInElements = document.querySelectorAll('.card, .alert');
    fadeInElements.forEach((el, index) => {
        setTimeout(() => {
            el.classList.add('fade-in');
        }, index * 100);
    });

    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({ behavior: 'smooth' });
            }
        });
    });

    // Dynamic table row highlighting
    const tableRows = document.querySelectorAll('.table tbody tr');
    tableRows.forEach(row => {
        row.addEventListener('click', () => {
            tableRows.forEach(r => r.classList.remove('table-active'));
            row.classList.add('table-active');
        });
    });

    // Form input animation
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.addEventListener('focus', () => {
            input.parentElement.classList.add('input-focused');
        });
        input.addEventListener('blur', () => {
            if (!input.value) {
                input.parentElement.classList.remove('input-focused');
            }
        });
    });

    // Modal animation
    const modals = document.querySelectorAll('.modal');
    modals.forEach(modal => {
        modal.addEventListener('show.bs.modal', () => {
            modal.querySelector('.modal-content').style.animation = 'modalFadeIn 0.3s ease';
        });
    });
});

// Animation keyframes (used in custom.css)
const styles = `
@keyframes modalFadeIn {
    from { opacity: 0; transform: scale(0.95); }
    to { opacity: 1; transform: scale(1); }
}
.fade-in {
    animation: fadeIn 0.5s ease-in-out;
}
.input-focused {
    position: relative;
}
.input-focused::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 2px;
    background-color: var(--accent-color);
    animation: underline 0.3s ease forwards;
}
@keyframes underline {
    from { width: 0; }
    to { width: 100%; }
}
`;
document.head.insertAdjacentHTML('beforeend', `<style>${styles}</style>`);