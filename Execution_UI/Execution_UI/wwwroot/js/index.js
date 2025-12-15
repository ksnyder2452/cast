/**
 * CAST Landing Page - Scripts
 * This file contains all JavaScript functionality for the CAST landing page
 * (Pages/Index.cshtml)
 * 
 * Features:
 * - Tab navigation and content switching
 */

document.addEventListener('DOMContentLoaded', () => {
  const tabButtons = document.querySelectorAll('.tab-btn');
  const tabContents = document.querySelectorAll('.tab-content');

  tabButtons.forEach(button => {
    button.addEventListener('click', () => {
      const tabId = button.getAttribute('data-tab');

      // Remove active class from all buttons and contents
      tabButtons.forEach(btn => {
        btn.classList.remove('active');
        btn.setAttribute('aria-selected', 'false');
      });
      tabContents.forEach(content => content.classList.remove('active'));

      // Add active class to clicked button and corresponding content
      button.classList.add('active');
      button.setAttribute('aria-selected', 'true');
      document.getElementById(tabId).classList.add('active');
    });
  });
});
