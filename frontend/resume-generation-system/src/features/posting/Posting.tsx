import { useParams } from "react-router-dom";
import styles from "./Posting.module.css";
import { useEffect, useState } from "react";

const BACKEND_URL = import.meta.env.VITE_BACKEND_URL;
type PostingBody = { id: string, postingText: string, importedAt: string, resumeUrl: string }

export const Posting = () => {
  const { postingId } = useParams();

  const [posting, setPosting] = useState<PostingBody | undefined>(undefined);

  useEffect(() => {
    let ignore = false;

    const fetchItems = async () => {
      try {
        const response = await fetch(`${BACKEND_URL}/api/GetCompletedPosting?completedPostingId=${postingId}`,);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        console.log(data);
        if (!ignore) setPosting(data);
      } catch (error) {
        console.log(error);
      }
    };

    fetchItems();

    return () => { ignore = true };
  }, [postingId]);

  if (!posting) {
    return <div>Loading items...</div>;
  }

  return (<article className={styles.posting}>
    <h1>{posting.id}</h1>
    <p>{posting.importedAt}</p>
    <p>{posting.postingText}</p>
    <a href={posting.resumeUrl}>Download Resume</a>
  </article>);
}