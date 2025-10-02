import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import './App.css'
import { Resume } from './features/resume/Resume'
import { PostingsList } from './features/postingsList/PostingsList';
import { Posting } from './features/posting/Posting';

const router = createBrowserRouter([
  { path: "/", element: <PostingsList /> },
  { path: "/index.html", element: <PostingsList /> },
  { path: "/posting/:postingId", element: <Posting /> },
  { path: "/resume", element: <Resume /> }
])

function App() {
  return <RouterProvider router={router} />
}

export default App
