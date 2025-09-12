document.addEventListener('DOMContentLoaded', () => {
    const searchTrigger = document.getElementById('searchTrigger');
    const searchPopupContainer = document.getElementById('searchPopupContainer');

    if (!searchTrigger || !searchPopupContainer) {
        console.error('Search trigger or container not found');
        return;
    }

    fetch('search-popup.html')
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.text();
        })
        .then(html => {
            searchPopupContainer.innerHTML = html;
            const searchPopup = document.getElementById('searchPopup');
            const closeSearchPopup = document.getElementById('closeSearchPopup');
            const searchInput = document.getElementById('searchInput');

            // Show popup on trigger click
            searchTrigger.addEventListener('click', () => {
                searchPopup.classList.add('active');
                searchInput.focus();
            });

            // Close popup
            closeSearchPopup.addEventListener('click', () => {
                searchPopup.classList.remove('active');
            });

            searchInput.addEventListener('blur', () => {
                setTimeout(() => {
                    searchPopup.classList.remove('active');
                }, 200);
            });

            // Handle search input (placeholder logic)
            searchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    const searchValue = searchInput.value;
                    console.log(`Searched: ${searchValue}`); // Replace with actual search logic
                    searchInput.value = '';
                    searchPopup.classList.remove('active');
                }
            });

            // Handle search item selection (placeholder)
            document.querySelectorAll('.search-item').forEach(item => {
                item.addEventListener('click', () => {
                    const gameName = item.querySelector('span').textContent;
                    console.log(`Selected: ${gameName}`); // Replace with poster update logic
                    searchPopup.classList.remove('active');
                });
            });
        })
        .catch(error => console.error('Error loading search popup:', error));
});