// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Mobile menu handling with improved animations
document.addEventListener('DOMContentLoaded', function() {
    const navbarCollapse = document.getElementById('navbarMain');
    const navbarToggler = document.querySelector('.navbar-toggler');
    
    if (navbarCollapse && navbarToggler) {
        // Toggle body scroll when menu is open
        navbarCollapse.addEventListener('show.bs.collapse', function() {
            document.body.style.overflow = 'hidden';
            // Add a slight delay for smoother animation
            requestAnimationFrame(() => {
                navbarToggler.setAttribute('aria-expanded', 'true');
            });
        });
        
        navbarCollapse.addEventListener('hide.bs.collapse', function() {
            document.body.style.overflow = '';
            navbarToggler.setAttribute('aria-expanded', 'false');
        });
        
        navbarCollapse.addEventListener('hidden.bs.collapse', function() {
            // Reset menu item animations when menu is fully closed
            const navItems = navbarCollapse.querySelectorAll('.nav-item');
            navItems.forEach(item => {
                item.style.transitionDelay = '0s';
            });
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
                        // Small delay for visual feedback before closing
                        setTimeout(() => {
                            bsCollapse.hide();
                        }, 150);
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

        // Touch swipe to close menu
        let touchStartX = 0;
        let touchEndX = 0;

        navbarCollapse.addEventListener('touchstart', function(e) {
            touchStartX = e.changedTouches[0].screenX;
        }, { passive: true });

        navbarCollapse.addEventListener('touchend', function(e) {
            touchEndX = e.changedTouches[0].screenX;
            handleSwipe();
        }, { passive: true });

        function handleSwipe() {
            const swipeThreshold = 50;
            if (touchStartX - touchEndX > swipeThreshold) {
                // Swiped left - close menu
                const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                if (bsCollapse && navbarCollapse.classList.contains('show')) {
                    bsCollapse.hide();
                }
            }
        }
    }
});