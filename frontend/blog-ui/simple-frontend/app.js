const API_URL = 'http://localhost:5055/api/blogs';  // YOUR LIVE API
let currentEditId = null;

document.addEventListener('DOMContentLoaded', () => {
    loadPosts();
    document.getElementById('savePostBtn').onclick = savePost;
    document.getElementById('confirmDeleteBtn').onclick = confirmDelete;
    document.getElementById('searchInput').oninput = debounce(searchPosts, 300);
});

function addNewPost() {
    resetForm();
    document.getElementById('postModalLabel').textContent = 'Add New Post';
    new bootstrap.Modal(document.getElementById('postModal')).show();
}

async function loadPosts(page = 1, author = '') {
    showLoading(true);
    try {
        const params = new URLSearchParams({ page, pageSize: 10 });
        if (author) params.append('author', author);
        const res = await fetch(`${API_URL}?${params}`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const posts = await res.json();
        renderPosts(posts);
    } catch (err) {
        showError('Failed to load posts: ' + err.message);
    } finally {
        showLoading(false);
    }
}

function renderPosts(posts) {
    const container = document.getElementById('blogPosts');
    if (posts.length === 0) {
        container.innerHTML = '<div class="col-12 text-center py-5"><h4>No posts found</h4></div>';
        return;
    }

    container.innerHTML = posts.map(p => `
        <div class="col-md-6 col-lg-4 mb-4">
            <div class="card blog-card h-100">
                <div class="card-body">
                    <h5 class="card-title">${escape(p.title)}</h5>
                    <h6 class="card-subtitle mb-2 text-muted">By ${escape(p.author)}</h6>
                    <p class="card-text">${escape(p.content).substring(0, 150)}...</p>
                </div>
                <div class="card-footer d-flex justify-content-between">
                    <small class="text-muted">${formatDate(p.createdAt)}</small>
                    <div>
                        <button class="btn btn-sm btn-outline-primary me-1" onclick="editPost(${p.id})">Edit</button>
                        <button class="btn btn-sm btn-outline-danger" onclick="deletePost(${p.id})">Delete</button>
                    </div>
                </div>
            </div>
        </div>
    `).join('');
}

async function savePost() {
    const id = document.getElementById('postId').value;
    const title = document.getElementById('title').value.trim();
    const author = document.getElementById('author').value.trim();
    const content = document.getElementById('content').value.trim();

    if (!title || !author || !content) {
        alert('All fields are required!');
        return;
    }

    const post = { title, author, content };
    if (id) post.id = parseInt(id);

    const method = id ? 'PUT' : 'POST';
    const url = id ? `${API_URL}/${id}` : API_URL;

    try {
        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(post)
        });

        if (res.ok) {
            bootstrap.Modal.getInstance(document.getElementById('postModal')).hide();
            resetForm();
            loadPosts();
            showSuccess(`Post ${id ? 'updated' : 'created'}!`);
        } else {
            const err = await res.text();
            showError('Save failed: ' + err);
        }
    } catch (err) {
        showError('Network error');
    }
}

function editPost(id) {
    fetch(`${API_URL}/${id}`)
        .then(r => r.json())
        .then(p => {
            document.getElementById('postId').value = p.id;
            document.getElementById('title').value = p.title;
            document.getElementById('author').value = p.author;
            document.getElementById('content').value = p.content;
            document.getElementById('postModalLabel').textContent = 'Edit Post';
            new bootstrap.Modal(document.getElementById('postModal')).show();
        })
        .catch(() => showError('Failed to load post'));
}

function deletePost(id) {
    currentEditId = id;
    new bootstrap.Modal(document.getElementById('deleteModal')).show();
}

async function confirmDelete() {
    try {
        const res = await fetch(`${API_URL}/${currentEditId}`, { method: 'DELETE' });
        if (res.ok) {
            bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
            loadPosts();
            showSuccess('Post deleted!');
        }
    } catch (err) {
        showError('Delete failed');
    }
}

function resetForm() {
    document.getElementById('postForm').reset();
    document.getElementById('postId').value = '';
}

function searchPosts() {
    const query = document.getElementById('searchInput').value.trim();
    loadPosts(1, query);
}

function showLoading(show) {
    document.getElementById('loading').style.display = show ? 'block' : 'none';
}

function showSuccess(msg) {
    createAlert(msg, 'success');
}

function showError(msg) {
    createAlert('Error: ' + msg, 'danger');
}

function createAlert(msg, type) {
    const alert = document.createElement('div');
    alert.className = `alert alert-${type} alert-dismissible fade show`;
    alert.innerHTML = `${msg}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
    document.querySelector('.container').insertBefore(alert, document.querySelector('.container').firstChild);
    setTimeout(() => alert.remove(), 5000);
}

function escape(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatDate(date) {
    return new Date(date).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

function debounce(func, wait) {
    let timeout;
    return (...args) => {
        clearTimeout(timeout);
        timeout = setTimeout(() => func(...args), wait);
    };
}