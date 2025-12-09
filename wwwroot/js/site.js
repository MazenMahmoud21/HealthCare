// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Mobile menu handling
document.addEventListener('DOMContentLoaded', function() {
    const navbarCollapse = document.getElementById('navbarMain');
    const navbarToggler = document.querySelector('.navbar-toggler');
    
    if (navbarCollapse && navbarToggler) {
        // Toggle body scroll when menu is open
        navbarCollapse.addEventListener('show.bs.collapse', function() {
            document.body.style.overflow = 'hidden';
        });
        
        navbarCollapse.addEventListener('hide.bs.collapse', function() {
            document.body.style.overflow = '';
        });

        // Close menu when clicking on the overlay (pseudo-element area)
        navbarCollapse.addEventListener('click', function(e) {
            // Check if click is outside the menu content (on the overlay area)
            const rect = navbarCollapse.getBoundingClientRect();
            if (e.clientX > rect.width) {
                const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                if (bsCollapse) {
                    bsCollapse.hide();
                }
            }
        });

        // Close menu when clicking a nav link (for better UX)
        const navLinks = navbarCollapse.querySelectorAll('.nav-link');
        navLinks.forEach(link => {
            link.addEventListener('click', function() {
                if (window.innerWidth < 992) {
                    const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                    if (bsCollapse) {
                        bsCollapse.hide();
                    }
                }
            });
        });

        // Handle escape key to close menu
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape' && navbarCollapse.classList.contains('show')) {
                const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                if (bsCollapse) {
                    bsCollapse.hide();
                }
            }
        });
    }
});