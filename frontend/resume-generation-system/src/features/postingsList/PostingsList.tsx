import { useEffect, useState } from "react";
import styles from "./PostingsList.module.css";

const BACKEND_URL = import.meta.env.VITE_BACKEND_URL;

type PostingHeader = { id: string, importedAt: string }
type PostingBody = { id: string, PostingText: string, ImportedAt: string, ResumeUrl: string }

export const PostingsList = () => {
  const [postings, setPostings] = useState<PostingHeader[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | undefined>(undefined);

  useEffect(() => {
    let ignore = false;

    const fetchItems = async () => {
      try {
        const response = await fetch(`${BACKEND_URL}/api/ListCompletedPostings`);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        if (!ignore) setPostings(data);
      } catch (error) {
        console.log(error);
        if (error instanceof Error) setError(error.message);
      } finally {
        setLoading(false);
      }
    };

    fetchItems();

    return () => { ignore = true };
  }, []);

  if (loading) {
    return <div>Loading items...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  return (<ul className={styles.postingsList}>
    {postings.map(posting => <li key={posting.id}><a href={`/posting/${posting.id}`}>{posting.id} at {posting.importedAt}</a></li>)}
  </ul>);
}